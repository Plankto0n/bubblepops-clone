using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BubbleClone;

public class SliderScript : MonoBehaviour
{
    private GameManager gm;

    public Image imageLeft;
    public Image imageRight;
    public Image fill;

    public Slider slider;

    private int lvlCounter = 0;
    private int pointsRequired = 15000;

    private void Awake()
    {
        gm = GameManager.Instance;
    }
    void Start()
    {
        lvlCounter = 1;
        slider.value = 0;
        slider.maxValue = pointsRequired;
        GetColor();
    }
    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.E))
        {
            slider.value += 1000;
        }
    }
    void GetColor()
    {
        imageLeft.color = gm.colorSet[(int)Mathf.Pow(2, lvlCounter + 1)];
        fill.color = gm.colorSet[(int)Mathf.Pow(2, lvlCounter + 1)];
        imageRight.color = gm.colorSet[(int)Mathf.Pow(2, lvlCounter + 2)];
    }

    void RankUp()
    {
        lvlCounter += 1;
        int randomthing = Random.Range((int)Mathf.Pow(2, 4 + (lvlCounter / 10)), (int)Mathf.Pow(2, 5 + (lvlCounter / 10)));
        int value = randomthing * 1000;
        pointsRequired += value;
        slider.minValue = slider.value;
        slider.maxValue = pointsRequired;
        GetColor();
    }

    public void Points()
    {
        if (slider.value >= pointsRequired)
            RankUp();
    }

}
