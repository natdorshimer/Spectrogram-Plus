import math
import sys
import argparse
from matplotlib import pyplot as plt

def plotShiftWindow(b1, b2, shift_index, order, thresh, fftsize, freq_res):
    """
    Plots the index shift attenuation and the index shift using the raised cosine shifting algorithm used in SpecPlus's frequency shifter
    """
    b_c = (b1+b2)//2
    omega = math.acos(math.pow(thresh, 1/order))/(b_c - b1)

    data = []
    xs = []
    if shift_index > 1:
        for n in range(b1, fftsize):
            if abs(angle := omega*(n-b_c)) <= (math.pi/2):
                data.append(math.pow(math.cos(angle), order))
                xs.append(n)

    elif shift_index < 0:
        for n in range(0,b2+1):
            if abs(angle := omega*(n-b_c)) <= (math.pi/2):
                data.append(math.pow(math.cos(omega*(angle)), order))
                xs.append(n)

    data_shifted = [int(shift_index * a) for a in data]
    xs = sorted(xs)
    plt.figure(0)
    plt.scatter(xs, data,)
    plt.ylabel('Amplitude')
    plt.xlabel("n")
    plt.title(f"Shift Attenuation Plot \n cos({round(omega,2)}*(n-{b_c}))^{order}\n b1: {b1}, b2: {b2}, shift: {shift_index}, Order:  {order}, thresh:  {thresh}")

    plt.figure(1)
    plt.scatter(xs, data_shifted)
    if shift_index < 0:
        plt.gca().invert_yaxis()

    plt.ylabel('Shifted Indices')
    plt.xlabel('n')
    plt.title(f"Index Shift Plot \n \
                b1: {b1}, b2:  {b2}, shift: {shift_index}, Order:  {order}, thresh:  {thresh}")
    plt.show()
    
def main(argv):
    """
    Plots how far each index shifts in the frequency dependent shifter algorithm
    Arguments: 
        -f1 : frequency of botton of selected window in hz
        -f2 : frequency of the top of the selected window in hz
        -shift : frequency in hz you want to shift the window
        -order : power to which the raised cosine is raised
        -thresh : value at which the ends of the selected window will be attenuated
        -fftsize : Number of fft points 
        -SR : Sample rate
        
    """
    pa = argparse.ArgumentParser()
    pa.add_argument('-f1',)
    pa.add_argument('-f2')
    pa.add_argument('-shift')
    pa.add_argument('-order')
    pa.add_argument('-thresh')
    pa.add_argument('-fftsize')
    pa.add_argument('-SR')
    args = pa.parse_args(argv)
    print(args.thresh)

    order = int(args.order)
    thresh = float(args.thresh)
    fftsize = int(args.fftsize)
    sample_rate = int(args.SR)

    freq_res = int((sample_rate / fftsize))
    b1 = int(args.f1) // freq_res
    b2 = int(args.f2) // freq_res
    shift_index = int(args.shift)//freq_res
    plotShiftWindow(b1, b2, shift_index, order, thresh, fftsize, freq_res)
    


if __name__ == "__main__":
    main(sys.argv[1:])