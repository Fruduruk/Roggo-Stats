import matplotlib.pyplot as plt

daten = [
    ["Alice", "24", "Informatik", "Aktiv"],
    ["Bob", "27", "Mathematik", "Inaktiv"],
    ["Clara", "22", "Physik", "Aktiv"],
]

spalten = ["Name", "Alter", "Studiengang", "Status"]

fig, ax = plt.subplots(figsize=(8, 2.8))
fig.patch.set_facecolor("#1e1e1e")
ax.set_facecolor("#1e1e1e")
ax.axis("off")

tabelle = ax.table(
    cellText=daten,
    colLabels=spalten,
    loc="center",
    cellLoc="center",
)

tabelle.auto_set_font_size(False)
tabelle.set_fontsize(11)
tabelle.scale(1.2, 1.7)

for (zeile, spalte), zelle in tabelle.get_celld().items():
    zelle.set_edgecolor("#3c3c3c")
    zelle.set_linewidth(0.8)

    if zeile == 0:
        zelle.set_facecolor("#2d2d30")
        zelle.get_text().set_color("#ffffff")
        zelle.get_text().set_weight("bold")
    else:
        zelle.set_facecolor("#252526")
        zelle.get_text().set_color("#d4d4d4")

        if spalte == 0:
            zelle.get_text().set_weight("bold")

        if spalte == 3:
            status = zelle.get_text().get_text()
            if status == "Aktiv":
                zelle.set_facecolor("#1f4d2e")
                zelle.get_text().set_weight("bold")
            elif status == "Inaktiv":
                zelle.set_facecolor("#5a1d1d")
                zelle.get_text().set_weight("bold")

plt.tight_layout()
plt.show()