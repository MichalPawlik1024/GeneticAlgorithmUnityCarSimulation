# This is a sample Python script.

# Press Shift+F10 to execute it or replace it with your code.
# Press Double Shift to search everywhere for classes, files, tool windows, actions, and settings.
import sys
from xxlimited_35 import Str

import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.lines import lineStyles


def scoresPlot(dataFrame, fpath : str):
    fig, (avgScoreAx,bestScoreAx) = plt.subplots(1,2, figsize = (19.2,10.8))

    avgSeries = dataFrame['avg_score'].values
    bestSeries = dataFrame['best_score'].values
    gensSeries = dataFrame['round'].values

    avgScoreAx.set_ylim(min(avgSeries.min() ,bestSeries.min() ), max(avgSeries.max(),bestSeries.max()))
    avgScoreAx.grid(color='black', linestyle='-', linewidth=1)
    avgScoreAx.plot(gensSeries, avgSeries)
    avgScoreAx.set_xlabel("Generation")
    avgScoreAx.set_ylabel("Avg score")
    avgScoreAx.set_title("Avarage score of population")

    bestScoreAx.grid(color='black',linestyle='-',linewidth=1)
    bestScoreAx.set_ylim(min(avgSeries.min(),bestSeries.min() ), max(avgSeries.max(),bestSeries.max()))
    bestScoreAx.plot(gensSeries,bestSeries)
    bestScoreAx.set_xlabel("Generation")
    bestScoreAx.set_ylabel("Best score")
    bestScoreAx.set_title("Best score in generations")
    plt.tight_layout()
    plt.savefig(fpath)



def valuesPlot(dataFrame, fpath: str):
    fig, ax = plt.subplots(1, 1, figsize=(19.2, 10.8))

    gensSeries = dataFrame['round'].values

    params = [
        'best_turnThreshold',
        'best_accelerateThreshold',
        'best_decelerateThreshold',
        'best_steerValue',
        'best_accelerateValue',
        'best_decelerateValue',
    ]

    for param in params:
        ax.plot(gensSeries, dataFrame[param].values, label=param)

    ax.set_xlabel("Generation")
    ax.set_ylabel("Value")
    ax.set_title("Best individual parameters over generations")
    ax.legend()
    ax.grid(color='black', linestyle='-', linewidth=1)

    plt.tight_layout()
    plt.savefig(fpath)


if __name__ == '__main__':
    inputFileName = ""
    outputFileName= "defaultOutput.png"
    if len(sys.argv) >= 3:
        inputFileName = str(sys.argv[1])
        outputFileName = str(sys.argv[2])
    else:
        sys.exit("Not enough args provided! ")
    if not inputFileName.endswith('.csv'):
        sys.exit("Wrong input file extension")
    if not outputFileName.endswith('.png'):
        sys.exit("Wrong output file extension")
    try:
        dataFrame = pd.read_csv(inputFileName,sep=",",decimal=".")
        base = outputFileName.rstrip(".png")
        scoresPlot(dataFrame, base + "Scores.png")
        valuesPlot(dataFrame, base + "Values.png")


    except FileNotFoundError:
        print("Failed to read and process file")
