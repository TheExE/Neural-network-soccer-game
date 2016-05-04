using UnityEngine;
using System.Collections;

public class NeuralNetworkConst
{
    public static double BIAS = -1;
    public static double ACTIVATION_TRESHOLD = 0.5;
    public static double MAX_PERTURBATION = 0.3;
    public static int NUMBER_OF_ELITE = 5;
    public static int NUMBER_OF_ELITE_COPYS = 5;
    public static double CROSSOVER_RATE = 0.7;
    public static double MUTATION_RATE = 0.1;
    public static float FITNESS_FOR_GOAL = 1;


    public static int ATTACKER_INPUT_COUNT = 2;
    public static int ATTACKER_OUTPUT_COUNT = 2;
    public static int ATTACKER_HID_LAYER_COUNT = 1;
    public static int ATTACKER_NEURONS_PER_HID_LAY = 4;


    public static int GOLY_INPUT_COUNT = 4;
    public static int GOLY_OUTPUT_COUNT = 4;
    public static int GOLY_HID_LAYER_COUNT = 3;
    public static int GOLY_NEURONS_PER_HID_LAY = 5;

    public static int DEFENSE_INPUT_COUNT = 6;
    public static int DEFENSE_OUTPUT_COUNT = 5;
    public static int DEFENSE_HID_LAYER_COUNT = 3;
    public static int DEFENSE_NEURONS_PER_HID_LAY = 7;

    public static int TOURNAMENT_COMPETITIORS = 5;
    public static int MAX_TICKS = 2000;
    	
}
