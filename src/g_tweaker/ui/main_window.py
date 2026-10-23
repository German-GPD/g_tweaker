from __future__ import annotations
import platform
import subprocess
from PySide6.QtCore import Qt
from PySide6.QtWidgets import (
    QMainWindow, QWidget, QHBoxLayout, QVBoxLayout, QLabel, QPushButton,
    QListWidget, QListWidgetItem, QStackedWidget, QLineEdit, QFrame,
    QMessageBox, QScrollArea, QTextEdit, QStatusBar
)
from g_tweaker.core.tweak_engine import TWEAKS, Tweak, apply_tweak, current_state, rollback_latest, history_items, activation_status
from g_tweaker.ui.theme import DARK_QSS

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("G_Tweaker")
        self.resize(1160, 740)
        self.setMinimumSize(900, 580)
        self.setStyleSheet(DARK_QSS)
        root = QWidget()
        root_layout = QHBoxLayout(root)
        root_layout.setContentsMargins(0, 0, 0, 0)
        root_layout.setSpacing(0)
        self.nav = QListWidget()
        self.nav.setFixedWidth(210)
        for name in ["Home", "All tweaks", "Privacy", "Performance", "Input", "Explorer", "Gaming", "Activation", "Profiles", "History"]:
            QListWidgetItem(name, self.nav)
        root_layout.addWidget(self.nav)
        body = QVBoxLayout()
        body.setContentsMargins(26, 22, 26, 16)
        body.setSpacing(14)
        self.search = QLineEdit()
        self.search.setPlaceholderText("Search tweaks…")
        self.search.setClearButtonEnabled(True)
        body.addWidget(self.search)
        self.pages = QStackedWidget()
        body.addWidget(self.pages, 1)
        root_layout.addLayout(body, 1)
        self.setCentralWidget(root)
        self.setStatusBar(QStatusBar())
        self._build_pages()
        self.nav.currentRowChanged.connect(self.show_page)
        self.search.textChanged.connect(self.refresh_tweaks)
        self.nav.setCurrentRow(0)

    def _page(self):
        page = QWidget()
        layout = QVBoxLayout(page)
        layout.setContentsMargins(0, 0, 0, 0)
        layout.setSpacing(14)
        return page

    def _heading(self, page, title, subtitle):
        label = QLabel(title)
        label.setObjectName("pageTitle")
        page.layout().addWidget(label)
        sub = QLabel(subtitle)
        sub.setObjectName("muted")
        sub.setWordWrap(True)
        page.layout().addWidget(sub)

    def _card(self, page, title, description):
        card = QFrame()
        card.setObjectName("card")
        box = QVBoxLayout(card)
        heading = QLabel(title)
        heading.setStyleSheet("font-size: 12pt; font-weight: 600;")
        desc = QLabel(description)
        desc.setObjectName("muted")
        desc.setWordWrap(True)
        box.addWidget(heading)
        box.addWidget(desc)
        page.layout().addWidget(card)

    def _button(self, layout, label, callback, primary=False):
        button = QPushButton(label)
        if primary:
            button.setObjectName("primary")
        button.clicked.connect(callback)
        layout.addWidget(button)
        return button

    def _build_pages(self):
        home = self._page()
        self._heading(home, "G_Tweaker", "Your Windows settings, in one place.")
        self._card(home, "Review before applying", "Each tweak changes one setting. Previous values are recorded so the latest operation can be rolled back.")
        self._card(home, "Early development", "Start with a small set of reversible settings. Some policies depend on Windows edition and may require administrator rights.")
        row = QHBoxLayout()
        self._button(row, "Browse all tweaks", lambda: self.nav.setCurrentRow(1), True)
        self._button(row, "Activation status", lambda: self.nav.setCurrentRow(7))
        home.layout().addLayout(row)
        self.pages.addWidget(home)

        self.tweak_page = self._page()
        self.tweak_title = QLabel("All tweaks")
        self.tweak_title.setObjectName("pageTitle")
        self.tweak_page.layout().addWidget(self.tweak_title)
        subtitle = QLabel("Check the current state and review the description before making changes.")
        subtitle.setObjectName("muted")
        subtitle.setWordWrap(True)
        self.tweak_page.layout().addWidget(subtitle)
        scroll = QScrollArea()
        scroll.setWidgetResizable(True)
        self.tweak_content = QWidget()
        self.tweak_list = QVBoxLayout(self.tweak_content)
        self.tweak_list.setAlignment(Qt.AlignmentFlag.AlignTop)
        self.tweak_list.setSpacing(10)
        scroll.setWidget(self.tweak_content)
        self.tweak_page.layout().addWidget(scroll, 1)
        self.pages.addWidget(self.tweak_page)

        activation = self._page()
        self._heading(activation, "Activation", "Check licence status and open official Windows settings.")
        self.activation_text = QLabel(activation_status())
        self.activation_text.setWordWrap(True)
        activation.layout().addWidget(self.activation_text)
        row = QHBoxLayout()
        self._button(row, "Refresh status", lambda: self.activation_text.setText(activation_status()), True)
        self._button(row, "Open Activation settings", self.open_activation)
        activation.layout().addLayout(row)
        self._card(activation, "Licensing", "G_Tweaker does not bypass Windows licensing or install unofficial activators.")
        self.pages.addWidget(activation)

        profiles = self._page()
        self._heading(profiles, "Profiles", "Profile preview and batch application are being built.")
        self._card(profiles, "Privacy", "Planned: preview privacy settings and apply them as a reviewed set.")
        self._card(profiles, "Gaming", "Planned: a small, transparent set of gaming-related options.")
        self._card(profiles, "Custom profiles", "Planned: save, import and export your own sets of tweaks.")
        self.pages.addWidget(profiles)

        history = self._page()
        self._heading(history, "Change history", "Rollback restores the saved previous value for the latest operation.")
        self._button(history.layout(), "Roll back latest change", self.rollback, True)
        self.history_text = QTextEdit()
        self.history_text.setReadOnly(True)
        history.layout().addWidget(self.history_text, 1)
        self.pages.addWidget(history)

    def show_page(self, index):
        if index == 0:
            self.pages.setCurrentIndex(0)
        elif 1 <= index <= 6:
            self.pages.setCurrentWidget(self.tweak_page)
            names = {1: "All tweaks", 2: "Privacy", 3: "Performance", 4: "Input", 5: "Explorer", 6: "Gaming"}
            self.tweak_title.setText(names[index])
            self.refresh_tweaks()
        elif index == 7:
            self.pages.setCurrentIndex(2)
            self.activation_text.setText(activation_status())
        elif index == 8:
            self.pages.setCurrentIndex(3)
        else:
            self.pages.setCurrentIndex(4)
            self.refresh_history()

    def refresh_tweaks(self, *_):
        if self.pages.currentWidget() != self.tweak_page:
            return
        while self.tweak_list.count():
            item = self.tweak_list.takeAt(0)
            widget = item.widget()
            if widget:
                widget.deleteLater()
        category = self.tweak_title.text()
        query = self.search.text().casefold().strip()
        for tweak in TWEAKS:
            if category != "All tweaks" and tweak.category != category:
                continue
            searchable = f"{tweak.title} {tweak.description} {tweak.category}".casefold()
            if query and query not in searchable:
                continue
            self._add_tweak_card(tweak)
        self.tweak_list.addStretch(1)

    def _add_tweak_card(self, tweak: Tweak):
        card = QFrame()
        card.setObjectName("card")
        box = QVBoxLayout(card)
        title = QLabel(tweak.title)
        title.setStyleSheet("font-size: 12pt; font-weight: 600;")
        desc = QLabel(tweak.description)
        desc.setObjectName("muted")
        desc.setWordWrap(True)
        state = QLabel("Current state: " + current_state(tweak))
        state.setObjectName("muted")
        box.addWidget(title)
        box.addWidget(desc)
        box.addWidget(state)
        if tweak.warning:
            warning = QLabel("ⓘ " + tweak.warning)
            warning.setWordWrap(True)
            warning.setStyleSheet("color: #e9bd69;")
            box.addWidget(warning)
        actions = QHBoxLayout()
        self._button(actions, "Apply", lambda checked=False, t=tweak: self.confirm_apply(t, True), True)
        self._button(actions, "Restore configured value", lambda checked=False, t=tweak: self.confirm_apply(t, False))
        actions.addStretch(1)
        box.addLayout(actions)
        self.tweak_list.addWidget(card)

    def confirm_apply(self, tweak, enabled):
        operation = "apply" if enabled else "restore the configured value for"
        answer = QMessageBox.question(self, "Confirm change",
            f"{operation} “{tweak.title}”?\\n\\nCurrent state: {current_state(tweak)}\\n\\nThe previous registry value will be saved for rollback.",
            QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.Cancel,
            QMessageBox.StandardButton.Cancel)
        if answer != QMessageBox.StandardButton.Yes:
            return
        try:
            apply_tweak(tweak, enabled)
            self.statusBar().showMessage("Setting updated.", 5000)
        except Exception as exc:
            QMessageBox.critical(self, "Could not apply setting", str(exc))
        self.refresh_tweaks()

    def rollback(self):
        answer = QMessageBox.question(self, "Rollback", "Restore the previous value for the latest recorded change?")
        if answer != QMessageBox.StandardButton.Yes:
            return
        try:
            title = rollback_latest()
            QMessageBox.information(self, "Rollback complete", f"Restored previous value for: {title}")
        except Exception as exc:
            QMessageBox.warning(self, "Rollback unavailable", str(exc))
        self.refresh_history()
        self.refresh_tweaks()

    def refresh_history(self):
        records = history_items()
        if not records:
            self.history_text.setPlainText("No changes have been recorded yet.")
        else:
            lines = [f"{r.get('timestamp', '')} — {r.get('title', r.get('tweak_id', 'Unknown'))} → {r.get('written_value')!r}" for r in records]
            self.history_text.setPlainText("\\n".join(lines))

    def open_activation(self):
        if platform.system() != "Windows":
            QMessageBox.information(self, "Windows only", "Activation settings are available on Windows.")
            return
        try:
            subprocess.Popen(["cmd.exe", "/c", "start", "", "ms-settings:activation"], shell=False)
        except OSError as exc:
            QMessageBox.warning(self, "Could not open settings", str(exc))
