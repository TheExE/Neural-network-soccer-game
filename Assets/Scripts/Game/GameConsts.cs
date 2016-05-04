using UnityEngine;
using System.Collections;

public class GameConsts
{
    public const float FRICTION = 0.2f;
    public const float ACCELERATED_TIME = 10;
    public const int MAX_GENERATIONS = 1500;
    public const int ATTACK_PLAYER_COUNT = 60;
    public const int DEFENSE_PLAYER_COUNT = 60;
    public const int GOALLY_PLAYER_COUNT = 60;
    public const float BALL_HIT_STRENGHT_SCALE = 20f;

    public const float GAME_FIELD_LEFT = -8.6f;
    public const float GAME_FIELD_RIGHT = 8.5f;
    public const float GAME_FIELD_UP = 3.8f;
    public const float GAME_FIELD_DOWN = -3.8f;
    public const float GOAL_UP = 0.85f;
    public const float GOAL_DOWN = -0.85f;

    public const string DEFENSE_PLAYER = "DEFENSE";
    public const string ATTACK_PLAYER = "ATTACK";
    public const string GOALLY_PLAYER = "GOALLY";
}
