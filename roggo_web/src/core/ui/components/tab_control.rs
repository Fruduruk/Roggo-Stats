use std::fmt::Display;

use eframe::egui::{self};

use crate::core::ui::{animation::ContextAnimationExt, theme::colors::colors, widgets::tab_button::tab_button};

#[derive(Default, Copy, Clone, PartialEq, enum_iterator::Sequence)]
pub enum Tab {
    #[default]
    Date,
    Session,
    Match,
    AllTime,
}

impl Display for Tab {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Tab::Date => write!(f, "Date"),
            Tab::Session => write!(f, "Session"),
            Tab::Match => write!(f, "Match"),
            Tab::AllTime => write!(f, "All Time"),
        }
    }
}

#[derive(Default)]
pub struct TabControl {
    selected: Tab,
}

impl TabControl {
    pub fn ui(&mut self, ui: &mut egui::Ui) -> Tab {
        let mut selected_rect = None;
        egui::Frame::new()
            .fill(colors(ui).surface)
            .corner_radius(5.0)
            .shadow(egui::Shadow {
                offset: [2, 2],
                blur: 2,
                spread: 1,
                color: colors(ui).surface_shadow,
            })
            .show(ui, |ui| {
                ui.horizontal(|ui| {
                    for tab in enum_iterator::all::<Tab>() {
                        let response = if tab == Tab::AllTime {
                            ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                                tab_button(ui, tab.to_string(), self.selected == tab)
                            })
                            .inner
                        } else {
                            tab_button(ui, tab.to_string(), self.selected == tab)
                        };

                        if response.clicked() {
                            self.selected = tab;
                        }

                        if self.selected == tab {
                            selected_rect = Some(response.rect);
                        }
                    }
                });
            });

        if let Some(rect) = selected_rect {
            let padding = 16.0;

            let target_left = rect.left() + padding;
            let target_right = rect.right() - padding;




            let left = ui.ctx().animate_value_with_time_and_easing(
                egui::Id::new("tab_underline_left"),
                target_left,
                0.3,
                egui::emath::easing::cubic_in_out,
            );

            let right = ui.ctx().animate_value_with_time_and_easing(
                egui::Id::new("tab_underline_right"),
                target_right,
                0.3,
                egui::emath::easing::cubic_in_out,
            );

            let y = rect.bottom() - 2.0;

            ui.painter().line_segment(
                [egui::pos2(left, y), egui::pos2(right, y)],
                egui::Stroke::new(2.0, ui.visuals().selection.bg_fill),
            );
        }

        self.selected
    }
}
