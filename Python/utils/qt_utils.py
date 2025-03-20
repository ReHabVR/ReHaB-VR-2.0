from PyQt6.QtWidgets import QApplication, QWidget, QPushButton, QVBoxLayout, QHBoxLayout, QGroupBox, QComboBox, QSlider, QLabel
from PyQt6.QtCore import Qt
from PyQt6.QtCore import QTimer
import SenderLib
from random import random
from functools import partial
from utils import GameControl as GmCtrl

class python_ui:
    # Potential values
    potential_ips = ['127.0.0.1', '192.168.1.33']
    potential_ports = ['25002', '25003']

    # Camera setup variables
    fov = 90
    rotation = 90
    offset_x = 0
    offset_y = 0

    # Connection setup variables
    host = potential_ips[0]
    port = int(potential_ports[0])
    is_connected = False
    turn = False
    gameControl = GmCtrl.GameControl()

    # Task setup variables
    tasks = ['Exercise 1', 'Exercise 2', 'Exercise 3', 'Exercise 4', 'Exercise 5', 'Moving Hand']
    task_buttons = []
    task_selected = 5

    def __init__(self):
        self.timer = QTimer()
        self.timer.timeout.connect(self.broadcast_event)
        self.initialize_game_control()

    def initialize_game_control(self):
        self.gameControl.applyMode = True
        self.gameControl.dataAcquisition = True
        self.gameControl.mode = self.task_selected

    def generate_ip_choices(self):
        groupbox = QGroupBox('Connection config')
        groupbox_layout = QVBoxLayout()
        groupbox.setLayout(groupbox_layout)

        self.connect_button = QPushButton('Connect')
        self.connect_button.clicked.connect(self.connect_button_handler)

        combobox_ips = QComboBox()
        combobox_ips.currentTextChanged.connect(self.update_ip)
        combobox_ips.addItems(self.potential_ips)

        combobox_ports = QComboBox()
        combobox_ports.currentTextChanged.connect(self.update_port)
        combobox_ports.addItems(self.potential_ports)

        groupbox_layout.addWidget(combobox_ips)
        groupbox_layout.addWidget(combobox_ports)
        groupbox_layout.addWidget(self.connect_button)

        return groupbox

    def generate_task_selector(self):
        groupbox = QGroupBox('Task selector')
        groupbox_layout = QVBoxLayout()
        groupbox.setLayout(groupbox_layout)

        for i in self.tasks:
            btn = QPushButton(i)
            btn.clicked.connect(partial(self.change_exercise_button_handler, i))
            if self.tasks.index(i) is self.task_selected:
                btn.setEnabled(False)
            groupbox_layout.addWidget(btn)
            self.task_buttons.append(btn)


        return groupbox

    def generate_camera_config(self):
        groupbox = QGroupBox('Camera config')

        groupbox_layout = QVBoxLayout()
        groupbox.setLayout(groupbox_layout)

        fov_slider, self.fov_label = self.generate_slider([50, 120], 1, 90, 'FOV', self.update_fov)
        rotation_slider, self.rotation_label = self.generate_slider([70, 120], 1, 90, 'Rotation', self.update_rotation)
        offset_x_slider, self.offset_x_label = self.generate_slider([-20, 20], 1, 0, 'Offset x', self.update_offset_x)
        offset_y_slider, self.offset_y_label = self.generate_slider([-20, 20], 1, 0, 'Offset y', self.update_offset_y)

        groupbox_layout.addWidget(self.fov_label)
        groupbox_layout.addWidget(fov_slider)

        groupbox_layout.addWidget(self.rotation_label)
        groupbox_layout.addWidget(rotation_slider)

        groupbox_layout.addWidget(self.offset_x_label)
        groupbox_layout.addWidget(offset_x_slider)

        groupbox_layout.addWidget(self.offset_y_label)
        groupbox_layout.addWidget(offset_y_slider)

        return groupbox

    def generate_app(self):
        app = self.initialize_app()
        widgets = [
            self.generate_ip_choices(),
            self.generate_camera_config(),
            self.generate_task_selector()
        ]
        window = QWidget()
        layout = QHBoxLayout()
        for widget in widgets:
            layout.addWidget(widget)
        window.setLayout(layout)
        window.show()
        app.exec()

    def initialize_app(self):
        app = QApplication([])
        return app

    def update_fov(self, value):
        self.fov = value
        self.fov_label.setText(f'FOV: {value}')

    def update_rotation(self, value):
        self.rotation = value
        self.rotation_label.setText(f'Rotation: {value}')

    def update_offset_x(self, value):
        self.offset_x = value
        self.offset_x_label.setText(f'Offset x: {value}')

    def update_offset_y(self, value):
        self.offset_y = value
        self.offset_y_label.setText(f'Offset y: {value}')

    def generate_slider(self, range, step, value, label, function):
        slider = QSlider(Qt.Orientation.Horizontal)
        slider.setRange(range[0], range[1])
        slider.setSingleStep(step)
        slider.setValue(value)

        label = QLabel(f'{label}: {value}')
        slider.valueChanged.connect(function)

        return slider, label

    def update_ip(self, value):
        self.host = value

    def update_port(self, value):
        self.port = int(value)

    def connect_button_handler(self, value):
        self.is_connected = not self.is_connected

        if self.is_connected is True:
            self.sender = SenderLib.Sender(self.host, self.port)

            self.timer.start(1000)
            self.connect_button.setText('Disconnect')
        else:
            self.timer.stop()
            self.connect_button.setText('Connect')

    def broadcast_event(self):
        print("Movement" if self.turn else "Rest")
        self.gameControl.left = self.turn
        self.gameControl.leftProbability = random()
        self.gameControl.camera_config.fov = self.fov
        self.gameControl.camera_config.rotation = self.rotation
        self.gameControl.camera_config.offset_x = self.offset_x
        self.gameControl.camera_config.offset_y = self.offset_y
        self.gameControl.mode = self.task_selected

        try:
            self.sender.send_data(self.gameControl)
        except ConnectionResetError:
            self.sender = SenderLib.Sender(self.host, self.port)

        self.turn = not self.turn

    def change_exercise_button_handler(self, label):
        new_task_selected = self.tasks.index(label)
        self.task_buttons[self.task_selected].setEnabled(True)
        self.task_selected = new_task_selected
        self.task_buttons[new_task_selected].setEnabled(False)

