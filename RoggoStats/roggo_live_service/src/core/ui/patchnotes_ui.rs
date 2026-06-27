use eframe::egui;
use egui_commonmark::{CommonMarkCache, CommonMarkViewer};

pub struct PatchNotesUi {
    patch_notes: Vec<(String, String)>,
    cache: CommonMarkCache,
}

impl PatchNotesUi {
    pub fn get_patch_versions(&self) -> Vec<&String> {
        self.patch_notes
            .iter()
            .map(|(version, _)| version)
            .collect()
    }
}

impl Default for PatchNotesUi {
    fn default() -> Self {
        Self {
            patch_notes: generate_patch_notes(),
            cache: Default::default(),
        }
    }
}

fn generate_patch_notes() -> Vec<(String, String)> {
    vec![
        (
            "0.5.0".into(),
            include_str!("../../../patch_notes/v0.5.0.md").into(),
        ),
        (
            "0.4.0".into(),
            include_str!("../../../patch_notes/v0.4.0.md").into(),
        ),
    ]
}

impl PatchNotesUi {
    pub fn show(&mut self, ui: &mut eframe::egui::Ui, since_version: String) {
        let mut concatenated = String::new();

        for (version, notes) in &self.patch_notes {
            if version == &since_version {
                break;
            }
            concatenated.push('\n');
            concatenated.push_str(notes);
        }
        CommonMarkViewer::new().show(ui, &mut self.cache, &concatenated);
    }
}
