use eframe::egui;

#[derive(Clone, Copy, Debug, PartialEq)]
pub struct ThemeColors {
    pub background: egui::Color32,
    pub surface: egui::Color32,
    pub surface_alt: egui::Color32,
    pub surface_hover: egui::Color32,
    pub surface_shadow: egui::Color32,

    pub primary: egui::Color32,
    pub primary_hover: egui::Color32,
    pub on_primary: egui::Color32,

    pub secondary: egui::Color32,
    pub secondary_hover: egui::Color32,
    pub on_secondary: egui::Color32,

    pub accent: egui::Color32,
    pub accent_hover: egui::Color32,
    pub on_accent: egui::Color32,

    pub text: egui::Color32,
    pub text_weak: egui::Color32,
    pub border: egui::Color32,

    pub success: egui::Color32,
    pub warning: egui::Color32,
    pub error: egui::Color32,
}

pub const DARK: ThemeColors = ThemeColors {
    background: egui::Color32::from_rgb(27, 27, 30),

    surface: egui::Color32::from_rgb(34, 34, 38),
    surface_alt: egui::Color32::from_rgb(34, 34, 38),
    surface_hover: egui::Color32::from_rgb(42, 42, 47),
    surface_shadow: egui::Color32::from_rgba_unmultiplied_const(13, 13, 24, 120),

    primary: egui::Color32::from_rgb(39, 46, 90),
    primary_hover: egui::Color32::from_rgb(100, 140, 235),
    on_primary: egui::Color32::WHITE,

    secondary: egui::Color32::from_rgb(34, 34, 66),
    secondary_hover: egui::Color32::from_rgb(165, 120, 235),
    on_secondary: egui::Color32::WHITE,

    accent: egui::Color32::from_rgb(146, 102, 37),
    accent_hover: egui::Color32::from_rgb(70, 210, 220),
    on_accent: egui::Color32::WHITE,

    text: egui::Color32::from_rgb(235, 235, 235),
    text_weak: egui::Color32::from_rgb(160, 160, 165),

    border: egui::Color32::from_rgb(55, 55, 60),

    success: egui::Color32::from_rgb(80, 190, 120),
    warning: egui::Color32::from_rgb(230, 175, 70),
    error: egui::Color32::from_rgb(220, 80, 80),
};

pub const LIGHT: ThemeColors = ThemeColors {
    background: egui::Color32::from_rgb(242, 242, 242),

    surface: egui::Color32::from_rgb(193, 193, 193),
    surface_alt: egui::Color32::from_rgb(238, 238, 242),
    surface_hover: egui::Color32::from_rgb(228, 228, 233),
    surface_shadow: egui::Color32::from_rgb(175, 175, 175),

    primary: egui::Color32::from_rgb(55, 95, 200),
    primary_hover: egui::Color32::from_rgb(45, 80, 180),
    on_primary: egui::Color32::WHITE,

    secondary: egui::Color32::from_rgb(120, 75, 190),
    secondary_hover: egui::Color32::from_rgb(100, 60, 170),
    on_secondary: egui::Color32::WHITE,

    accent: egui::Color32::from_rgb(20, 145, 160),
    accent_hover: egui::Color32::from_rgb(15, 125, 140),
    on_accent: egui::Color32::WHITE,

    text: egui::Color32::from_rgb(25, 25, 28),
    text_weak: egui::Color32::from_rgb(100, 100, 105),

    border: egui::Color32::from_rgb(210, 210, 215),

    success: egui::Color32::from_rgb(40, 145, 80),
    warning: egui::Color32::from_rgb(185, 125, 25),
    error: egui::Color32::from_rgb(190, 50, 50),
};

#[derive(Clone, Copy)]
struct ThemePalette {
    dark: ThemeColors,
    light: ThemeColors,
}

const DEFAULT_PALETTE: ThemePalette = ThemePalette {
    dark: DARK,
    light: LIGHT,
};

fn palette_id() -> egui::Id {
    egui::Id::new("roggo_theme_palette")
}

pub fn save_default_palette_in_ctx_data(ctx: &egui::Context) {
    ctx.data_mut(|data| {
        data.insert_temp(palette_id(), DEFAULT_PALETTE);
    });
}

fn palette(ctx: &egui::Context) -> ThemePalette {
    ctx.data(|data| {
        data.get_temp::<ThemePalette>(palette_id())
            .unwrap_or(DEFAULT_PALETTE)
    })
}

pub fn colors(ui: &egui::Ui) -> ThemeColors {
    let palette = palette(ui.ctx());

    match ui.theme() {
        egui::Theme::Dark => palette.dark,
        egui::Theme::Light => palette.light,
    }
}

pub fn default_colors(theme: egui::Theme) -> ThemeColors {
    match theme {
        egui::Theme::Dark => DARK,
        egui::Theme::Light => LIGHT,
    }
}

pub fn set_colors(ctx: &egui::Context, theme: egui::Theme, colors: ThemeColors) {
    let mut palette = palette(ctx);

    match theme {
        egui::Theme::Dark => palette.dark = colors,
        egui::Theme::Light => palette.light = colors,
    }

    ctx.data_mut(|data| {
        data.insert_temp(palette_id(), palette);
    });

    apply_colors(ctx, theme, &colors);

    ctx.request_repaint();
}

pub fn reset_colors(ctx: &egui::Context, theme: egui::Theme) {
    set_colors(ctx, theme, default_colors(theme));
}

pub fn apply_theme_colors(ctx: &egui::Context) {
    let palette = palette(ctx);

    apply_colors(ctx, egui::Theme::Dark, &palette.dark);

    apply_colors(ctx, egui::Theme::Light, &palette.light);
}

pub fn apply_colors(ctx: &egui::Context, theme: egui::Theme, colors: &ThemeColors) {
    ctx.style_mut_of(theme, |style| {
        let visuals = &mut style.visuals;

        // General surfaces
        visuals.panel_fill = colors.background;
        visuals.window_fill = colors.surface;

        visuals.faint_bg_color = colors.surface_alt;
        visuals.extreme_bg_color = colors.surface_alt;
        visuals.text_edit_bg_color = Some(colors.surface_alt);
        visuals.code_bg_color = colors.surface_alt;

        // Text
        visuals.override_text_color = Some(colors.text);
        visuals.weak_text_color = Some(colors.text_weak);

        // Selection / accent
        visuals.selection.bg_fill = colors.accent;
        visuals.selection.stroke = egui::Stroke::new(1.0, colors.on_accent);

        visuals.hyperlink_color = colors.accent;

        // Semantic colors
        visuals.warn_fg_color = colors.warning;
        visuals.error_fg_color = colors.error;

        // Non-interactive
        visuals.widgets.noninteractive.bg_fill = colors.surface;
        visuals.widgets.noninteractive.weak_bg_fill = colors.surface;
        visuals.widgets.noninteractive.bg_stroke = egui::Stroke::new(1.0, colors.border);
        visuals.widgets.noninteractive.fg_stroke = egui::Stroke::new(1.0, colors.text);

        // Normal widgets
        visuals.widgets.inactive.bg_fill = colors.surface;
        visuals.widgets.inactive.weak_bg_fill = colors.surface;
        visuals.widgets.inactive.bg_stroke = egui::Stroke::new(1.0, colors.border);
        visuals.widgets.inactive.fg_stroke = egui::Stroke::new(1.0, colors.text);

        // Hover
        visuals.widgets.hovered.bg_fill = colors.surface_hover;
        visuals.widgets.hovered.weak_bg_fill = colors.surface_hover;
        visuals.widgets.hovered.bg_stroke = egui::Stroke::new(1.0, colors.accent);
        visuals.widgets.hovered.fg_stroke = egui::Stroke::new(1.0, colors.text);

        // Active
        visuals.widgets.active.bg_fill = colors.surface_hover;
        visuals.widgets.active.weak_bg_fill = colors.surface_hover;
        visuals.widgets.active.bg_stroke = egui::Stroke::new(1.0, colors.primary);
        visuals.widgets.active.fg_stroke = egui::Stroke::new(1.0, colors.text);

        // Open dropdown/menu
        visuals.widgets.open.bg_fill = colors.surface_hover;
        visuals.widgets.open.weak_bg_fill = colors.surface_hover;
        visuals.widgets.open.bg_stroke = egui::Stroke::new(1.0, colors.accent);
        visuals.widgets.open.fg_stroke = egui::Stroke::new(1.0, colors.text);

        visuals.disabled_alpha = 0.45;
    });
}
