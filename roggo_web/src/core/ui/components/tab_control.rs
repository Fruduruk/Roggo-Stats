use std::fmt::Display;

use eframe::egui::{self};

use crate::core::ui::{theme::colors::colors, widgets::tab_button::tab_button};

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
        egui::Frame::new()
            .fill(colors(ui).surface)
            .corner_radius(5.0)
            .shadow(egui::Shadow {
                offset: [2,2],
                blur: 2,
                spread: 1,
                color: colors(ui).surface_shadow,
            })
            .show(ui, |ui| {
                ui.horizontal(|ui| {
                    for tab in enum_iterator::all::<Tab>() {
                        if tab == Tab::AllTime {
                            ui.with_layout(
                                egui::Layout::right_to_left(egui::Align::Center),
                                |ui| {
                                    if tab_button(ui, tab.to_string(), self.selected == tab)
                                        .clicked()
                                    {
                                        self.selected = tab;
                                    }
                                },
                            );
                        } else {
                            if tab_button(ui, tab.to_string(), self.selected == tab).clicked() {
                                self.selected = tab;
                            }
                        }
                    }
                });
            });

        self.selected
    }
}
