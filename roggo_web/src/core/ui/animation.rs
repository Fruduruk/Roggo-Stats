use eframe::egui;

#[derive(Clone, Copy)]
struct ValueAnimationState {
    from: f32,
    target: f32,
    current: f32,

    bool_target: bool,
    bool_start: f32,
    bool_animation_time: f32,
}

pub trait ContextAnimationExt {
    fn animate_value_with_time_and_easing(
        &self,
        id: egui::Id,
        target_value: f32,
        animation_time: f32,
        easing: fn(f32) -> f32,
    ) -> f32;
}

impl ContextAnimationExt for egui::Context {
    fn animate_value_with_time_and_easing(
        &self,
        id: egui::Id,
        target_value: f32,
        animation_time: f32,
        easing: fn(f32) -> f32,
    ) -> f32 {
        let state_id = id.with("value_animation_state");
        let bool_id = id.with("value_animation_bool");

        let Some(mut state) = self.data(|data| {
            data.get_temp::<ValueAnimationState>(state_id)
        }) else {
            self.animate_bool_with_time(
                bool_id,
                false,
                animation_time,
            );

            self.data_mut(|data| {
                data.insert_temp(
                    state_id,
                    ValueAnimationState {
                        from: target_value,
                        target: target_value,
                        current: target_value,

                        bool_target: false,
                        bool_start: 0.0,
                        bool_animation_time: animation_time,
                    },
                );
            });

            return target_value;
        };

        let mut bool_value = self.animate_bool_with_time(
            bool_id,
            state.bool_target,
            state.bool_animation_time,
        );

        let t = normalized_progress(
            bool_value,
            state.bool_start,
            state.bool_target,
        );

        state.current = egui::lerp(
            state.from..=state.target,
            easing(t),
        );

        if target_value != state.target {
            state.from = state.current;
            state.target = target_value;

            state.bool_start = bool_value;
            state.bool_target = !state.bool_target;

            let bool_end = if state.bool_target {
                1.0
            } else {
                0.0
            };

            let remaining_distance =
                (bool_end - bool_value).abs();

            state.bool_animation_time = if animation_time <= 0.0 {
                0.0
            } else {
                animation_time
                    / remaining_distance.max(f32::EPSILON)
            };

            bool_value = self.animate_bool_with_time(
                bool_id,
                state.bool_target,
                state.bool_animation_time,
            );

            let t = normalized_progress(
                bool_value,
                state.bool_start,
                state.bool_target,
            );

            state.current = egui::lerp(
                state.from..=state.target,
                easing(t),
            );

            self.request_repaint();
        }

        self.data_mut(|data| {
            data.insert_temp(state_id, state);
        });

        state.current
    }
}

fn normalized_progress(
    value: f32,
    start: f32,
    target: bool,
) -> f32 {
    let end = if target { 1.0 } else { 0.0 };

    let distance = end - start;

    if distance.abs() <= f32::EPSILON {
        1.0
    } else {
        ((value - start) / distance).clamp(0.0, 1.0)
    }
}