import matplotlib
def isCOLA(window, frames, stepSize):
    """
    Determines if a given window is COLA compliant

    COLA compliancy is defined as: Sum(w(n-mR),m) = 1, for all n in Integers
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


isCOLA(hann(33), 5, 16)