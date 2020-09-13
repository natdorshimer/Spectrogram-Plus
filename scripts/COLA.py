
def printCOLA(window, frames, stepSize):
    """
    Prints the recombination of the window function. The ideal is a combination of window, frames, and 
    stepsize to achieve uniformity of 1 over the entire array (except for end points)
    """

    data = [0 for i in range(len(window) + frames*stepSize)]
    for m in range(0,frames):
        frame_index = m*stepSize
        for j in range(len(window)):
            data[frame_index+j] += window[j]
    print(data)

import math
def hann(length):
    return [math.pow(math.sin(math.pi*i/length),2) for i in range(length)]

def sqrt_hann(length):
    return [(math.sin(math.pi*i/length)) for i in range(length)]


printCOLA(hann(33), 5, 16)