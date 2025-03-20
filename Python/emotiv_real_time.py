"""
==============================================================
RealTime processing for Emotiv EEG headset with use of bsl ('brain signals LSL')
==============================================================

"""
from math import ceil
import time
import socket
import time
import bsl.stream_viewer.stream_viewer
from matplotlib import pyplot as plt
import mne
# import stream_desc
import numpy as np
from scipy.signal import butter, sosfilt, sosfilt_zi

from bsl import StreamReceiver, StreamPlayer, datasets
from bsl.utils import Timer

# Filter
# ^^^^^^
#
# Data should be filtered along one dimension. For this example, a butter IIR
# filter is used. More information on filtering is available on the MNE
# documentation:
#
# - `Background information on filtering <https://mne.tools/stable/auto_tutorials/preprocessing/25_background_filtering.html#disc-filtering>`_
# - `Filtering and resampling data <https://mne.tools/stable/auto_tutorials/preprocessing/30_filtering_resampling.html#tut-filter-resample>`_

def create_bandpass_filter(low, high, fs, n):
    """
    Create a bandpass filter using a butter filter of order n.

    Parameters
    ----------
    low : float
        The lower pass-band edge.
    high : float
        The upper pass-band ege.
    fs : float
        Sampling rate of the data.
    n : int
        Order of the filter.

    Returns
    -------
    sos : array
        Second-order sections representation of the IIR filter.
    zi_coeff : array
        Initial condition for sosfilt for step response steady-state.
    """
    # Divide by the Nyquist frequency
    bp_low = low / (0.5 * fs)
    bp_high = high / (0.5 * fs)
    # Compute SOS output (second order sections)
    sos = butter(n, [bp_low, bp_high], btype='band', output='sos')
    # Construct initial conditions for sosfilt for step response steady-state.
    zi_coeff = sosfilt_zi(sos).reshape((sos.shape[0], 2, 1))

    return sos, zi_coeff

# EEG data is usually subject to a lage DC offset, which corresponds to a step
# response steady-state. The initial conditions are determined by multiplying
# the ``zi_coeff`` with the DC offset. The DC offset value can be approximated
# by taking the mean of a small window.

# %%
# Buffer
# ^^^^^^^
#
# When creating the filtered buffer, the duration has to be define to create a
# numpy array of the correct shape and pre-allocate the required space.

class Buffer:
    """
    A buffer containing filter data and its associated timestamps.

    Parameters
    ----------
    buffer_duration : float
        Length of the buffer in seconds.
    sr : bsl.StreamReceiver
        StreamReceiver connected to the desired data stream.
    """

    def __init__(self, buffer_duration, sr):
        # Store the StreamReceiver in a class attribute
        self.sr = sr

        # Retrieve sampling rate and number of channels
        self.fs = int(self.sr.streams[stream_name].sample_rate)
        self.nb_channels = len(self.sr.streams[stream_name].ch_list) - 1

        # Define duration
        self.buffer_duration = buffer_duration
        self.buffer_duration_samples = ceil(self.buffer_duration * self.fs)

        # Create data array
        self.timestamps = np.zeros(self.buffer_duration_samples)
        self.data = np.zeros((self.buffer_duration_samples, self.nb_channels))
        # For demo purposes, let's store also the raw data
        self.raw_data = np.zeros((self.buffer_duration_samples,
                                  self.nb_channels))

        # Create filter BP (1, 15) Hz and filter variables
        self.sos, self.zi_coeff = create_bandpass_filter(1., 30., self.fs, n=2)
        self.zi = None

    def update(self):
        """
        Update the buffer with new samples from the StreamReceiver. This method
        should be called regularly, with a period at least smaller than the
        StreamReceiver buffer length.
        """
        # Acquire new data points
        self.sr.acquire()
        data_acquired, ts_list = self.sr.get_buffer()
        self.sr.reset_buffer()

        if len(ts_list) == 0:
            return  # break early, no new samples

        # Remove trigger channel
        data_acquired = data_acquired[:, 1:]

        # Filter acquired data
        if self.zi is None:
            # Initialize the initial conditions for the cascaded filter delays.
            self.zi = self.zi_coeff * np.mean(data_acquired, axis=0)
        data_filtered, self.zi = sosfilt(self.sos, data_acquired, axis=0,
                                         zi=self.zi)

        # Roll buffer, remove samples exiting and add new samples
        self.timestamps = np.roll(self.timestamps, -len(ts_list))
        self.timestamps[-len(ts_list):] = ts_list
        self.data = np.roll(self.data, -len(ts_list), axis=0)
        self.data[-len(ts_list):, :] = data_filtered
        self.raw_data = np.roll(self.raw_data, -len(ts_list), axis=0)
        self.raw_data[-len(ts_list):, :] = data_acquired


stream_name = 'EmotivDataStream-EEG' #LSL stream name
buffer_duration = 1  # in seconds
sr = StreamReceiver(bufsize=0.2, winsize=0.2, stream_name=stream_name)
time.sleep(0.2)  # wait to fill LSL inlet.

#Show metadata of a stream
# stream_desc.description()
# Create buffer
buffer = Buffer(buffer_duration, sr)

#Get rid of initial 0s
timer = Timer()
while timer.sec() <= buffer_duration:
    buffer.update()

#plt.show()

idx_last_plot = 1
timer.reset()

host, port = "127.0.0.1", 25002
#host, port = "192.168.218.6", 25002
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((host, port))

startPos = [1, 0, 0]
turn = False

while True:
    buffer.update()
    # check if we just passed the 5s between plot limit
    if timer.sec() // 1 == idx_last_plot:
        # average all channels to simulate an evoked response
        #plt.plot(buffer.timestamps, buffer.raw_data[:,3], 'r')  # plotting t, a separately
        #plt.plot(buffer.timestamps, buffer.raw_data[:,4], 'g')  # plotting t, a separately
        #plt.plot(buffer.timestamps, buffer.raw_data[:,5], 'b')  # plotting t, a separately
        print(np.around(np.amax(buffer.data[:, 3:7]), decimals=3))
        if np.amax(buffer.data[:,3:7]) >= 50:
            turn = False
            print("Interference")
        else:
            turn = True

        posString = ','.join(map(str, startPos))
        # print(posString)

        sock.sendall(posString.encode("UTF-8"))
        receivedData = sock.recv(1024).decode("UTF-8")
        # print(receivedData)
        # startPos[1] = 1 if turn else 0
        startPos[1] = 0 if turn else 1

        #print(buffer.data[:,3:7])
        idx_last_plot += 1
        #plt.pause(0.01)
#plt.show()

del sr  # disconnects and close the LSL inlet.