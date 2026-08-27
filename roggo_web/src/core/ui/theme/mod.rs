pub mod geometry;
pub mod colors;
pub mod fonts;

use eframe::egui;


pub fn apply_theme(ctx: &egui::Context) {
    ctx.set_theme(egui::Theme::Dark);
    ctx.set_pixels_per_point(2.0);

    colors::save_default_palette_in_ctx_data(ctx);

    fonts::apply_fonts(ctx);
    colors::apply_theme_colors(ctx);
    geometry::apply_size_spacing_corners(ctx);
}

