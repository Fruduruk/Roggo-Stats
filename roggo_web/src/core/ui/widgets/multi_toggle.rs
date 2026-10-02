use std::fmt::Display;

use eframe::egui::{self, Stroke};

use crate::core::ui::{animation::ContextAnimationExt, theme::colors::colors};

const INNER_MARGIN: f32 = 5.0;
const CORNER_RADIUS: f32 = 5.0;
const ITEM_SPACING: f32 = 10.0;
const ANIMATION_TIME: f32 = 0.25;

struct Button<T> {
    state: T,
    text: String,
    rect: egui::Rect,
    response: egui::Response,
}

#[derive(Default, Copy, Clone, PartialEq)]
pub enum Orientation {
    #[default]
    Horizontal,
    Vertical,
}

#[derive(Default)]
pub struct MultiToggle<T>
where
    T: Copy + PartialEq + enum_iterator::Sequence + Display,
{
    new_state: Option<T>,
    selected: (T, bool),
}

impl<T> MultiToggle<T>
where
    T: Copy + PartialEq + enum_iterator::Sequence + Display,
{
    pub fn set_state(&mut self, new_state: T) {
        self.new_state = Some(new_state);
    }

    pub fn get_state(&self) -> (T, bool) {
        self.selected
    }

    pub fn ui(&mut self, ui: &mut egui::Ui, orientation: Orientation) {
        egui::Frame::new()
            .fill(colors(ui).surface_alt)
            .corner_radius(CORNER_RADIUS)
            .inner_margin(INNER_MARGIN)
            .show(ui, |ui| {
                self.show_toggle_buttons(ui, orientation);
            });
    }

    fn show_toggle_buttons(&mut self, ui: &mut egui::Ui, orientation: Orientation) {
        self.selected.1 = false;

        if let Some(new_state) = self.new_state.take() {
            if self.selected.0 != new_state {
                self.selected = (new_state, true);
            }
        }

        let buttons = self.sense_buttons(ui, orientation);

        if let Some(animated_rect) = self.get_animated_rect(ui, &buttons) {
            ui.painter()
                .rect_filled(animated_rect, CORNER_RADIUS, ui.visuals().selection.bg_fill);

            ui.painter().rect_stroke(
                animated_rect,
                CORNER_RADIUS,
                Stroke::new(2.0, colors(ui).border),
                egui::StrokeKind::Outside,
            );
        }

        for button in buttons {
            let selected = self.selected.0 == button.state;

            let text_color = if selected || button.response.hovered() {
                ui.visuals().strong_text_color()
            } else {
                ui.visuals().weak_text_color()
            };

            let mut font_id = egui::TextStyle::Button.resolve(ui.style());

            if button.response.hovered() {
                font_id.size += 0.5;
            }

            ui.painter().text(
                button.rect.center(),
                egui::Align2::CENTER_CENTER,
                button.text,
                font_id,
                text_color,
            );
        }
    }

    fn get_animated_rect(
        &mut self,
        ui: &mut egui::Ui,
        buttons: &[Button<T>],
    ) -> Option<egui::Rect> {
        let animation_id = ui.next_auto_id();

        let target_rect = buttons
            .iter()
            .find(|b| b.state == self.selected.0)
            .map(|b| b.rect.expand(INNER_MARGIN))?;

        let left = ui.ctx().animate_value_with_time_and_easing(
            animation_id.with("left"),
            target_rect.left(),
            ANIMATION_TIME,
            egui::emath::easing::cubic_in_out,
        );

        let right = ui.ctx().animate_value_with_time_and_easing(
            animation_id.with("right"),
            target_rect.right(),
            ANIMATION_TIME,
            egui::emath::easing::cubic_in_out,
        );

        let top = ui.ctx().animate_value_with_time_and_easing(
            animation_id.with("top"),
            target_rect.top(),
            ANIMATION_TIME,
            egui::emath::easing::cubic_in_out,
        );

        let bottom = ui.ctx().animate_value_with_time_and_easing(
            animation_id.with("bottom"),
            target_rect.bottom(),
            ANIMATION_TIME,
            egui::emath::easing::cubic_in_out,
        );

        Some(egui::Rect::from_min_max(
            egui::pos2(left, top),
            egui::pos2(right, bottom),
        ))
    }

    fn sense_buttons(&mut self, ui: &mut egui::Ui, orientation: Orientation) -> Vec<Button<T>> {
        let states = enum_iterator::all::<T>().collect::<Vec<_>>();
        let button_count = states.len();

        let mut buttons = vec![];

        match orientation {
            Orientation::Horizontal => {
                ui.horizontal(|ui| {
                    ui.spacing_mut().item_spacing.x = ITEM_SPACING;

                    let available_width = ui.available_width();

                    let total_spacing = ITEM_SPACING * button_count.saturating_sub(1) as f32;

                    let button_width = (available_width - total_spacing) / button_count as f32;

                    for state in states {
                        let text = state.to_string();

                        let font_id = egui::TextStyle::Button.resolve(ui.style());

                        let galley = ui.painter().layout_no_wrap(
                            text.clone(),
                            font_id,
                            ui.visuals().text_color(),
                        );

                        let (rect, response) = ui.allocate_exact_size(
                            egui::vec2(button_width, ui.spacing().interact_size.y),
                            egui::Sense::click(),
                        );

                        if response.clicked() && self.selected.0 != state {
                            self.selected = (state, true);
                        }

                        buttons.push(Button {
                            state,
                            text,
                            rect,
                            response,
                        });
                    }
                });
            }

            Orientation::Vertical => {
                ui.vertical(|ui| {
                    ui.spacing_mut().item_spacing.y = ITEM_SPACING;

                    for state in states {
                        let text = state.to_string();

                        let font_id = egui::TextStyle::Button.resolve(ui.style());

                        let galley = ui.painter().layout_no_wrap(
                            text.clone(),
                            font_id,
                            ui.visuals().text_color(),
                        );

                        let (rect, response) = ui.allocate_exact_size(
                            egui::vec2(ui.available_width(), ui.spacing().interact_size.y),
                            egui::Sense::click(),
                        );

                        if response.clicked() && self.selected.0 != state {
                            self.selected = (state, true);
                        }

                        buttons.push(Button {
                            state,
                            text,
                            rect,
                            response,
                        });
                    }
                });
            }
        }

        buttons
    }
}