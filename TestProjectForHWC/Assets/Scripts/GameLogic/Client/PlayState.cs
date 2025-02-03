using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;


public class PlayerData
{
    public int id = 0;
    public int HP = 100;
    public int regenirationStrength = 2; // Сила регенирации - параметр дял возможности изменения на сервере
    public int MaxHP = 100; // максимальное здоровье -         параметр дял возможности изменения на сервере
    public int PowerHit = 8; // Сила удара -                   параметр для возможности изменения на сервере
    public eFFect Effect = eFFect.none;
    public int[] EffectRemainder = new int[Enum.GetNames(typeof(eFFect)).Length]; // остаток наложенных эффектов / уровни массива это номера эффектов,а занчения это число их активных ходов\

    public PlayerData Clone()
    {
        return (PlayerData)this.MemberwiseClone();
    }
}

public class PlayState : MonoBehaviour
{
    public static int CountPlayerInSession = 1;
    public static bool IsActiveUI = false;
    public static bool PlayerTurn = false;
    public int ActivePlayer = -1;
    public static PlayerData[] Player = new PlayerData[2];

    public GameObject[] Card = new GameObject[5];
    public bool[] BOTCard = new bool[5];

    public Text Text, Text1;
    public bool[] ActiveCard = new bool[5];
    public bool[] BOTActiveCard = new bool[5];


    public int[] CardRecoil = new int[5];
    public int[] BOTCardRecoil = new int[5];

    public bool ActiveState = false;
    public int HodCount = 0;
    public Animator[] animCard = new Animator[5];
    public Animator animTable;


    public void Start()
    {
        PlayerData basePlayer = new PlayerData(), basePlayer2 = new PlayerData();
        Player[0] = basePlayer; Player[1] = basePlayer2;
        Player[1].id++;
        ActivePlayer = Player[0].id;

        Text1.text = HodCount.ToString();

        HOD(Player[0]);

    }

    public void Update()
    {
        Text.text = Player[0].HP.ToString() + "HP\t" + Player[1].HP + "HP";

        if(Input.GetKeyDown(KeyCode.E))
        {
            Enemy.SetActive++;
        }

    }


    public void HOD(PlayerData _Player) // действия эффектов перед началом хода игрока 
    {
        for (int Dingo = 1; Dingo < Enum.GetNames(typeof(eFFect)).Length; Dingo++) // начало отсчета с 1 , так как effect none не имеет степень счета хода
        {
            if (Player[_Player.id].EffectRemainder[Dingo] > 0)
            {
                string PROVERKA = Dingo.ToString();
                switch (PROVERKA)
                {
                    case "1":
                        Debug.Log("горение");
                        Player[_Player.id].EffectRemainder[Dingo]--;
                        Player[_Player.id].HP--;
                        break;

                    case "2":
                        Debug.Log("барьер");
                        Player[_Player.id].EffectRemainder[Dingo]--;
                        break;

                    case "3":
                        Debug.Log("регенирация");
                        if (Player[_Player.id].HP < 100)
                        {
                            for (int i = 0; i < Player[_Player.id].regenirationStrength; i++)
                            {
                                Player[_Player.id].HP++;
                            }
                            Player[_Player.id].EffectRemainder[Dingo]--;
                        }
                        else if (Player[_Player.id].HP == 100)
                        {
                            Player[_Player.id].EffectRemainder[Dingo] = 0;
                        }
                        break;

                    default:
                        Debug.Log($"на данный ID: {Dingo} эффекта - не существует сценария \n пропуск действия");
                        break;
                }
            }
        }
        if (ActivePlayer > 0)
        {
            Debug.Log("SDSDSDSDSDSDSDSD");
            StartCoroutine(WaitingAI());
            return;
        }
        else if ( ActivePlayer == 0)
        {
            ActiveMoment();
            return;
        }
    }

    public void ActiveMoment()
    {
        for (int Dingo = 1; Dingo < Card.Length; Dingo++)
        {
            if (!ActiveCard[Dingo])
            {
                if (CardRecoil[Dingo] == 0)
                {
                    Card[Dingo].SetActive(true);
                    animCard[Dingo].Play("CardOpen");
                    ActiveCard[Dingo] = true;
                }
                else if (CardRecoil[Dingo] > 0)
                {
                    CardRecoil[Dingo]--;
                }
            }
            else if (ActiveCard[Dingo] && CardRecoil[Dingo] != 0)
            {
                Card[Dingo].SetActive(false);
                ActiveCard[Dingo] = false;
                CardRecoil[Dingo]--;
            }
        }

        IsActiveUI = true;

    }

    // зона взаимодейсвия ↓

    public void Attack()
    {
        HodCount++;
        Text1.text = HodCount.ToString();
        if (ActivePlayer > 0)
        {
            animTable.Play("intro");

            for (int i = 0; i < Card.Length; i++)
            {
                Button _button = Card[i].GetComponent<Button>();
                _button.interactable = true;
            }

            if (Player[ActivePlayer - 1].HP > 0)
            {
                for (int Dingo = 0; Dingo < Player[ActivePlayer].PowerHit; Dingo++)
                {
                    Player[0].HP--;
                }
            }
            else
            {
                Debug.Log("ты безщадно погиб !");
            }
            ActivePlayer = 0;
        }
        else
        {
            animCard[2].Play("CardRotate");

            for (int i = 0; i < Card.Length; i++)
            {
                Button _button = Card[i].GetComponent<Button>();
                _button.interactable = false;
            }

            StartCoroutine(Waiting());

            if (Player[ActivePlayer + 1].HP > 0)
            {
                for (int Dingo = 0; Dingo < Player[ActivePlayer].PowerHit; Dingo++)
                {
                    Player[ActivePlayer + 1].HP--;
                }
            }
            else
            {
                Debug.Log("ПОБЕДА");
            }

            ActivePlayer++;
        }

        HOD(Player[ActivePlayer]);
    }

    public void FireBall()
    {
        HodCount++;
        Text1.text = HodCount.ToString();
        if (ActivePlayer > 0)
        {
            animTable.Play("intro");
            for (int i = 0; i < Card.Length; i++)
            {
                Button _button = Card[i].GetComponent<Button>();
                _button.interactable = true;
            }
            BOTCardRecoil[1] = 6;
            if (Player[ActivePlayer - 1].HP > 0)
            {
                for (int Dingo = 0; Dingo < 5; Dingo++)
                {
                    Player[ActivePlayer - 1].HP--;
                }
                Player[ActivePlayer - 1].EffectRemainder[1] = 5;
            }
            else
            {
                Debug.Log("помер - да только дым у угля и витает");
            }
            ActivePlayer = 0;
        }
        else
        {
            animCard[1].Play("CardClose");
            for (int i = 0; i < Card.Length; i++)
            {
                Button _button = Card[i].GetComponent<Button>();
                _button.interactable = false;
            }
            StartCoroutine(Waiting());
            CardRecoil[1] = 6;
            if (Player[ActivePlayer + 1].HP > 0)
            {
                for (int Dingo = 0; Dingo < 5; Dingo++)
                {
                    Player[ActivePlayer + 1].HP--;
                }
                Player[ActivePlayer + 1].EffectRemainder[1] = 5;
            }
            else
            {
                Debug.Log("ПОБЕДА");
            }
            ActivePlayer++;
        }
        HOD(Player[ActivePlayer]);
    }

    public void Barrier()
    {
        HodCount++;
        Text1.text = HodCount.ToString();
        if (ActivePlayer > 0)
        {
            animTable.Play("intro");
            for (int i = 0; i < Card.Length; i++)
            {
                Button _button = Card[i].GetComponent<Button>();
                _button.interactable = true;
            }
            BOTCardRecoil[3] = 4;
            Player[1].EffectRemainder[2] = 2;
            ActivePlayer = 0;
        }
        else
        animCard[3].Play("CardClose");
        for (int i = 0; i < Card.Length; i++)
        {
            Button _button = Card[i].GetComponent<Button>();
            _button.interactable = false;
        }
        StartCoroutine(Waiting());
        {
            CardRecoil[3] = 4;
            Player[0].EffectRemainder[2] = 2;
            ActivePlayer++;
        }
        HOD(Player[ActivePlayer]);
    }

    public void Regeniration()
    {
        HodCount++;
        Text1.text = HodCount.ToString();
        if (ActivePlayer > 0)
        {
            animTable.Play("intro");
            for (int i = 0; i < Card.Length; i++)
            {
                Button _button = Card[i].GetComponent<Button>();
                _button.interactable = true;
            }
            BOTCardRecoil[4] = 5;
            Player[1].EffectRemainder[3] = 3;
            ActivePlayer = 0;
        }
        else
        animCard[4].Play("CardClose");
        for (int i = 0; i < Card.Length; i++)
        {
            Button _button = Card[i].GetComponent<Button>();
            _button.interactable = false;
        }
        StartCoroutine(Waiting());
        {
            CardRecoil[4] = 5;
            Player[0].EffectRemainder[3] = 3;
            ActivePlayer++;
        }
        HOD(Player[ActivePlayer]);
    }

    public void Purifying()
    {
        HodCount++;
        Text1.text = HodCount.ToString();
        if (ActivePlayer > 0)
        {
            animTable.Play("intro");
            for (int i = 0; i < Card.Length; i++)
            {
                Button _button = Card[i].GetComponent<Button>();
                _button.interactable = true;
            }
            BOTCardRecoil[0] = 5;
            Player[1].EffectRemainder[1] = 0;
            ActivePlayer = 0;
        }
        else
        {
            animCard[0].Play("CardClose");
            for (int i = 0; i < Card.Length; i++)
            {
                Button _button = Card[i].GetComponent<Button>();
                _button.interactable = false;
            }
            StartCoroutine(Waiting());
            CardRecoil[0] = 5;
            Player[0].EffectRemainder[1] = 0;
            Debug.Log("ВЫ были очищенны");
            ActivePlayer++;
        }
        HOD(Player[ActivePlayer]);
    }

    public void BotPlay()
    {
        int RandomChose = UnityEngine.Random.Range(0, 5);
        if (!BOTCard[RandomChose])
        {
            RandomChose = 0;
        }
        ActiveMomentBOT(RandomChose);
    }

    public void ActiveMomentBOT(int _RandomChose)
    {
        for (int Dingo = 1; Dingo < BOTCard.Length; Dingo++)
        {
            if (!BOTActiveCard[Dingo])
            {
                if (BOTCardRecoil[Dingo] == 0)
                {
                    BOTCard[Dingo] = true;
                    BOTActiveCard[Dingo] = true;
                }
                else if (BOTCardRecoil[Dingo] > 0)
                {
                    BOTCardRecoil[Dingo]--;
                }
            }
            else if (BOTActiveCard[Dingo] && BOTCardRecoil[Dingo] != 0)
            {
                
                BOTCard[Dingo] = false;
                BOTActiveCard[Dingo] = false;
                BOTCardRecoil[Dingo]--;
            }
        }

        string Proverka = _RandomChose.ToString();
        switch (Proverka)
        {
            case "0":
                Attack();
                Debug.Log($"ATAKA {Proverka}");
                break;

            case "1":
                FireBall();
                Debug.Log($"Fireball {Proverka}");
                break;

            case "2":
                Barrier();
                Debug.Log($"Barrier {Proverka}");
                break;

            case "3":
                Regeniration();
                Debug.Log($"REGENIARTIOM {Proverka}");
                break;

            case "4":
                Purifying();
                Debug.Log($"OCHISTKA {Proverka}");
                break;
        }
    }

    IEnumerator Waiting()
    {
        yield return new WaitForSeconds(1f);
        animTable.Play("closeTable");
    }
    IEnumerator WaitingAI()
    {
        yield return new WaitForSeconds(2f);
        BotPlay();
    }
}

public enum eFFect
{
    none,
    berning,
    barrier,
    regeniration
}
