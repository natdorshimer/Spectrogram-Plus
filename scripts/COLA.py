import matplotlib.pyplot as plt
import statistics
import math
import sys
import os


def plotCOLA(window_name, window, frames, stepSize):

    """
    Plots the COLA sum of the given window and saves the plot to either 
    .\\COLA\\valid_windows\ or .\\COLA\\invalid_windows\
    """

    data, median, flatness = COLA_info(window, frames, stepSize)
    plt.plot(data,':')
    plt.ylabel('Amplitude')
    plt.xlabel("n")
    plt.title(f"COLA of {window_name} Window\n \
                Window Length: {len(window)}, Frames: {frames}, StepSize: {stepSize}, \n \
                Median:{median}, Flatness: {flatness}, Overlap: {round(stepSize / len(window), 3)*100}%")
                
    if not os.path.exists(".\\COLA Windows\\valid_windows\\"):
        os.mkdir(".\\COLA Windows\\valid_windows\\")
    if not os.path.exists(".\\COLA Windows\\invalid_windows\\"):
        os.mkdir(".\\COLA Windows\\invalid_windows\\")
    if median == 1:
        filename = f".\\COLA Windows\\valid_windows\\{window_name}_{len(window)}_{frames}_{stepSize}.PNG"
    else:
        filename = f".\\COLA Windows\\invalid_windows\\{window_name}_{len(window)}_{frames}_{stepSize}.PNG"

    plt.savefig(filename)
    os.startfile(filename) 

def isCOLA(window, stepSize, frames = 10):
    """
    Returns true if it's COLA over a given amount of frames
    """
    data, median, flatness = COLA_info(window, frames, stepSize)
    return median == 1

def COLA_info(window, frames, stepSize):
    """
    Returns:
        data : the COLA sum of a given window, frames, and overlap
        median: returns the median value of the COLA sum. 1.0 is a COLA window
        flatness: "Averaged" standard deviation to somewhat indicate flatness
    """
    
    # COLA sum
    data = [0 for i in range(len(window) + (frames-1)*stepSize)]
    for m in range(0, frames):
        frame_index = m*stepSize
        for j in range(len(window)):
            data[frame_index+j] += window[j]
    median = round(statistics.median(data),2) # If it's approximately 1, it's COLA

    # Flatness calculation
    # Relatively arbitrary, but meant to calculate how "flat" the main part of the curve is
    # Kinda like an average standard deviation across the frames
    flatness = 0
    avg_val = sum(data)/len(data)
    for i in range(len(data)):
        flatness += math.pow((data[i]-avg_val), 2)
    flatness = round(math.sqrt(flatness)/(frames), 2)

    return (data, median, flatness)

def print_COLA_info(window, frames, stepSize):
    """
    Prints data about a given window given its overlap and number of frames
    """
    data, median, flatness = COLA_info(window, frames, stepSize)
    print(f"COLA: {median == 1}")
    print(f"Median: {median}")
    print(f"Flatness: {flatness}")
    print(f"Overlap: {round(1-stepSize/len(window), 3)*100}%")

def hann(length):
    """returns a hanning window centered at length/2 with N = length"""
    return [math.pow(math.sin(math.pi*i/length),2) for i in range(length)]

def root_hann(length):
    """returns the square root of a hanning window centered at length/2 with N = length"""
    return [(math.sin(math.pi*i/length)) for i in range(length)]

# Maps a shorthand name to its full name and its window
window_map = { 
    "hann" : ("Hanning", hann), 
    "roothann": ("Sqrt Hanning", root_hann)
    }

        
def main(args):
    """
    Command line argument structure: 
        python COLA.py window_type window_length frames stepSize -plot
        If -plot is used at the end then it will plot and save the overlap structure using plotCOLA

    Example:
        >> python COLA.py hann 1024 5 512 -plot
    
    See window_map for valid window_types
    """
    window_type = args[0]
    name = window_map[window_type][0]
    func = window_map[window_type][1]
    length = int(args[1])
    frames = int(args[2])
    stepSize = int(args[3])

    print_COLA_info(func(length), frames, stepSize)
    if len(args) >= 5 and args[4] == "-plot":
        plotCOLA(name, func(length), frames, stepSize)


if __name__ == "__main__":
    main(sys.argv[1:])

