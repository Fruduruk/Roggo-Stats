use eframe::egui;

use crate::core::ui::{app::COMPATIBLE_AGENT_VERSION, patchnotes_ui::PatchNotesUi};

pub const DOWNLOAD_URL: &'static str = "https://github.com/Fruduruk/Roggo-Stats/releases/download/roggo-agent-v0.5.0/RoggoAgentSetup_0.5.0.exe";

#[derive(Default)]
enum ViewMode {
    #[default]
    Info,
    Download,
    PatchNotes,
}

#[derive(Default)]
pub struct InstallUi {
    view_mode: ViewMode,
    pub patch_notes_ui: PatchNotesUi,
}

impl InstallUi {
    pub fn ui(&mut self, ui: &mut eframe::egui::Ui, version: String) {
        egui::Panel::left("info_nav_list")
            .resizable(false)
            .default_size(150.)
            .show_inside(ui, |ui| {
                if ui
                    .add_sized([ui.available_width(), 32.0], egui::Button::new("Overview"))
                    .clicked()
                {
                    self.view_mode = ViewMode::Info;
                }
                if ui
                    .add_sized([ui.available_width(), 32.0], egui::Button::new("Source"))
                    .clicked()
                {
                    self.view_mode = ViewMode::Download;
                }
                if ui
                    .add_sized(
                        [ui.available_width(), 32.0],
                        egui::Button::new("Patch Notes"),
                    )
                    .clicked()
                {
                    self.view_mode = ViewMode::PatchNotes;
                }
            });
        match self.view_mode {
            ViewMode::Info => self.show_info(ui, version),
            ViewMode::Download => self.show_download(ui, version),
            ViewMode::PatchNotes => self.show_patch_notes(ui, version),
        };
    }

    fn show_patch_notes(&mut self, ui: &mut egui::Ui, version: String) {
        egui::CentralPanel::default().show_inside(ui, |ui| {
            ui.heading("Patch Notes:");
            egui::ScrollArea::vertical().show(ui, |ui| {
                self.patch_notes_ui.show(ui, version);
            });
        });
    }

    fn show_info(&mut self, ui: &mut egui::Ui, version: String) {
        egui::CentralPanel::default().show_inside(ui, |ui| {
            ui.heading("Welcome to Roggo Stats!");
            ui.separator();

            ui.heading("What's new?");
            egui::ScrollArea::vertical().show(ui, |ui| {
                self.patch_notes_ui.show(ui, version);
            });
        });
    }

    fn show_download(&mut self, ui: &mut egui::Ui, version: String) {
        egui::CentralPanel::default().show_inside(ui, |ui| {
            // ui.label(format!(
            //     "Please download and run roggo agent with version {}",
            //     COMPATIBLE_AGENT_VERSION
            // ));
            ui.hyperlink_to(DOWNLOAD_URL, DOWNLOAD_URL);
        });
    }
}
