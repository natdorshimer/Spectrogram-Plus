import matplotlib.pyplot as plt
import math
import sys
import os


def plotCOLA(window_name, window, frames, stepSize):

    """
    Plots the COLA of the given window and saves the plot to either 
    .\COLA\valid_windows or .\COLA\invalid_windows
    """

    data = [0 for i in range(len(window) + frames*stepSize)]
    for m in range(0,frames+1):
        frame_index = m*stepSize
        for j in range(len(window)):
            data[frame_index+j] += window[j]

    variance = 0
    start = min(len(window)//2, stepSize)
    end = max(len(data)-stepSize, max(len(window)//2, len(window)-stepSize))
    for i in range(start, end):
        variance += math.pow(data[i]-1, 2)
    std_dev = round(math.sqrt(variance), 4) 
            
    plt.plot(data)
    plt.ylabel('Amplitude')
    plt.xlabel("n")
    plt.title(f"COLA of {window_name} Window\n \
                Window Length: {len(window)}, Frames: {frames}, StepSize: {stepSize}, StdDev: {std_dev}")
                
    current_dir = os.path.abspath(".")
    if std_dev == 0.0:
        filename = f"{current_dir}\COLA\\valid_windows\{window_name}_{len(window)}_{frames}_{stepSize}.PNG"
    else:
        filename = f"{current_dir}\COLA\invalid_windows\{window_name}_{len(window)}_{frames}_{stepSize}.PNG"

    plt.savefig(filename)
    os.startfile(filename) 


def hann(length):
    return [math.pow(math.sin(math.pi*i/length),2) for i in range(length)]

def root_hann(length):
    return [(math.sin(math.pi*i/length)) for i in range(length)]

# Maps a shorthand name to its full name and its window
window_map = { 
    "hann" : ("Hanning", hann), 
    "roothann": ("Sqrt Hanning", root_hann)
    }
              
def main(args):
    """
    Command line argument structure: 
    python COLA.py window_type window_length frames stepSize
    """
    window_type = args[0]
    name = window_map[window_type][0]
    func = window_map[window_type][1]
    length = int(args[1])
    frames = int(args[2])
    stepSize = int(args[3])

    plotCOLA(name, func(length), frames, stepSize)
    os.system("")

if __name__ == "__main__":
    main(sys.argv[1:])

