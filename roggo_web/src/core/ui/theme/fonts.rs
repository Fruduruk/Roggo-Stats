use eframe::egui;

pub fn apply_fonts(ctx: &egui::Context) {
    let mut fonts = egui::FontDefinitions::default();

    fonts.font_data.insert(
        "montserrat".into(),
        egui::FontData::from_static(include_bytes!("../../../../assets/Montserrat-Medium.ttf"))
            .into(),
    );

    fonts.font_data.insert(
        "noto_sans".into(),
        egui::FontData::from_static(include_bytes!("../../../../assets/NotoSans-Light.ttf")).into(),
    );

    fonts.font_data.insert(
        "noto_cjk".into(),
        egui::FontData::from_static(include_bytes!("../../../../assets/NotoSansCJK-VF.ttf.ttc"))
            .into(),
    );

    fonts.font_data.insert(
        "noto_symbols".into(),
        egui::FontData::from_static(include_bytes!(
            "../../../../assets/NotoSansSymbols-Light.ttf"
        ))
        .into(),
    );

    fonts
        .families
        .get_mut(&egui::FontFamily::Proportional)
        .unwrap()
        .insert(0, "montserrat".into());

    fonts.families.insert(
        egui::FontFamily::Name("player_name".into()),
        vec![
            "montserrat".to_owned(),
            "noto_sans".to_owned(),
            "noto_cjk".to_owned(),
            "noto_symbols".to_owned(),
        ],
    );

    ctx.set_fonts(fonts);
}
