use eframe::egui;

pub fn apply_fonts(ctx: &egui::Context) {
    let mut fonts = egui::FontDefinitions::default();

    fonts.font_data.insert(
        "montserrat".into(),
        egui::FontData::from_static(include_bytes!("../../../../assets/Montserrat-Medium.ttf"))
            .into(),
    );

    fonts
        .families
        .get_mut(&egui::FontFamily::Proportional)
        .unwrap()
        .insert(0, "montserrat".into());

    ctx.set_fonts(fonts);
}
