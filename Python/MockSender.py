import socket
import json
import time

class GameControl:
    def __init__(self):
        self.movement = True
        self.left = False
        self.right = False
        self.leftProbability = 0.0
        self.rightProbability = 0.0
        self.applyMode = True
        self.mode = 0  # animation index
        self.dataAcquisition = True

    def to_json(self):
        return json.dumps(self.__dict__)


class GameState:
    def __init__(self, **entries):
        self.__dict__.update(entries)

    def __repr__(self):
        return f"<GameState {self.__dict__}>"


class Sender:
    def __init__(self, host="127.0.0.1", port=25002):
        self.host = host
        self.port = port

        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.connect((host, port))
        print(f"[Python] Connected to {host}:{port}")

    def send(self, control: GameControl) -> GameState:
        payload = control.to_json().encode("utf-8")
        self.sock.sendall(payload)

        response = self.sock.recv(4096).decode("utf-8")
        if not response:
            return None

        data = json.loads(response)
        return GameState(**data)

    def close(self):
        self.sock.close()


if __name__ == "__main__":
    sender = Sender()
    control = GameControl()
    try:
        mode = 0
        while True:
            control.mode = mode
            control.movement = True
            control.applyMode = True

            state = sender.send(control)

            print(
                f"[Python] Sent mode={control.mode} | "
                f"Received state={state}"
            )

            mode = 1
            time.sleep(1.0)

    except KeyboardInterrupt:
        print("\n[Python] Stopping test.")
    finally:
        sender.close()
