import sys
from PySide6.QtWidgets import QApplication
from g_tweaker.ui.main_window import MainWindow


def main():
    app = QApplication(sys.argv)
    app.setApplicationName('G_Tweaker')
    app.setOrganizationName('German-GPD')
    app.setStyle('Fusion')
    window = MainWindow()
    window.show()
    return app.exec()


if __name__ == '__main__':
    raise SystemExit(main())
