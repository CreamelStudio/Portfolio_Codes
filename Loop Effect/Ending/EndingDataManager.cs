using System.Collections.Generic;
using UnityEngine;

// 반환할 데이터 클래스
public class EndingData
{
    public string title;
    public string[] content;
}

public class EndingDataManager : MonoBehaviour
{
    // 현재 엔딩 ID (인스펙터 등에서 설정)
    public string currentEndingID;

    public static EndingDataManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 1. 타이틀 데이터 (User defined structure)
    public readonly Dictionary<string, Dictionary<Language, string>> endingTitles = new()
    {
        {
            "ENDING_08_DESTRUCTION", new()
            {
                { Language.Korean,  "파멸 (게임 오버)" },
                { Language.English, "Destruction (Game Over)" }
            }
        },
        {
            "ENDING_14_MECHANICAL_PARADISE", new()
            {
                { Language.Korean,  "기계의 낙원" },
                { Language.English, "Mechanical Paradise" }
            }
        },
        {
            "ENDING_15_FADING_LANTERN", new()
            {
                { Language.Korean,  "희미해지는 등불" },
                { Language.English, "Fading Lantern" }
            }
        },
        {
            "ENDING_01_MASKED_TYCOON", new()
            {
                { Language.Korean,  "가면 쓴 재벌 (배드 엔딩)" },
                { Language.English, "Masked Tycoon (Bad Ending)" }
            }
        },
        {
            "ENDING_02_RETURN_FOREST", new()
            {
                { Language.Korean,  "숲으로 돌아간 사람들 (배드 엔딩)" },
                { Language.English, "Return to the Forest (Bad Ending)" }
            }
        },
        {
            "ENDING_03_FORBIDDEN_CITY", new()
            {
                { Language.Korean,  "웃음이 금지된 도시 (배드 엔딩)" },
                { Language.English, "The City Where Laughter is Forbidden (Bad Ending)" }
            }
        },
        {
            "ENDING_04_LAWLESS_PLEASURE", new()
            {
                { Language.Korean,  "무법지대의 쾌락 (배드 엔딩)" },
                { Language.English, "Lawless Pleasure (Bad Ending)" }
            }
        },
        {
            "ENDING_05_COLD_STEEL", new()
            {
                { Language.Korean,  "차가운 강철 제국 (노멀 엔딩)" },
                { Language.English, "Cold Steel Empire (Normal Ending)" }
            }
        },
        {
            "ENDING_06_SLUM_UTOPIA", new()
            {
                { Language.Korean,  "슬럼가의 유토피아 (노멀 엔딩)" },
                { Language.English, "Slum Utopia (Normal Ending)" }
            }
        },
        {
            "ENDING_10_GLASS_GARDEN", new()
            {
                { Language.Korean,  "유리 정원 시민국가 (노멀 엔딩)" },
                { Language.English, "Glass Garden City-State (Normal Ending)" }
            }
        },
        {
            "ENDING_09_GOLDEN_DESERT", new()
            {
                { Language.Korean,  "황금의 사막 (배드 엔딩)" },
                { Language.English, "Golden Desert (Bad Ending)" }
            }
        },
        {
            "ENDING_11_BLUE_PRISON", new()
            {
                { Language.Korean,  "푸른 감옥 (배드 엔딩)" },
                { Language.English, "Blue Prison (Bad Ending)" }
            }
        },
        {
            "ENDING_12_TWILIGHT_SMILES", new()
            {
                { Language.Korean,  "미소의 황혼 (노멀 엔딩)" },
                { Language.English, "Twilight Smiles (Normal Ending)" }
            }
        },
        {
            "ENDING_07_PRECARIOUS_PEACE", new()
            {
                { Language.Korean,  "아슬아슬한 평화 (트루 엔딩)" },
                { Language.English, "Precarious Peace (True Ending)" }
            }
        },
        {
            "ENDING_13_COLORLESS_MID", new()
            {
                { Language.Korean,  "무색의 중간지대 (뉴트럴 엔딩)" },
                { Language.English, "Colorless Middle Ground (Neutral Ending)" }
            }
        }
    };

    // 2. 본문 텍스트 데이터 (User defined structure)
    public readonly Dictionary<string, Dictionary<Language, string[]>> endingTexts = new()
    {
        {
            "ENDING_08_DESTRUCTION", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "도대체 어디서부터 잘못된 걸까… 균형은 되돌릴 수 없을 만큼 무너져버렸다.",
                        "거리엔 비명이 가득하고, 불타는 건물들이 밤하늘을 붉게 물들인다.",
                        "사람들은 살아남기 위해 서로를 짓밟고, 이 도시는 오래전에 기능을 멈췄다.",
                        "이제는 나도, 그리고 당신―지도자인 당신도―더 이상 할 수 있는 게 없다.",
                        "미안하다. 모두를 지키고 싶었는데… 우리의 세계는 오늘로 끝이다."
                    }
                },
                {
                    // User provided English text
                    Language.English, new[]
                    {
                        "Where did it all go wrong... The balance has collapsed beyond recovery.",
                        "The streets are filled with screams, and burning buildings dye the night sky red.",
                        "People trample each other to survive, and this city ceased to function long ago.",
                        "Now, neither I nor you—the leader—can do anything anymore.",
                        "I'm sorry. I wanted to protect everyone... Our world ends today."
                    }
                }
            }
        },
        {
            "ENDING_14_MECHANICAL_PARADISE", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "AI는 도시를 완벽하게 계산했다. 실수는 이제 오류 코드로만 남아 있다.",
                        "아침 해는 정해진 각도로 떠오르고, 범죄율은 소수점 아래로 떨어졌다.",
                        "모든 시민은 표준화된 행복을 누리며, 불안은 백신처럼 제거되었다.",
                        "우리는 마침내 천국을 만들었다… 그런데 자유는 어디에 숨겨둔 걸까?",
                        "이 완벽하게 최적화된 삶 속에서, ‘선택’이라는 단어는 사라졌다.",
                        "사람들이 말하듯, 낙원은 지옥보다 훨씬 조용하다."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "The AI calculated the city perfectly. Mistakes now remain only as error codes.",
                        "The morning sun rises at a set angle, and the crime rate has dropped below the decimal point.",
                        "All citizens enjoy standardized happiness, and anxiety has been eliminated like a virus.",
                        "We finally created heaven... But where did we hide freedom?",
                        "In this perfectly optimized life, the word 'choice' has disappeared.",
                        "As people say, paradise is much quieter than hell."
                    }
                }
            }
        },
        {
            "ENDING_15_FADING_LANTERN", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "우리는 살아남았다. 하지만 이걸 정말 ‘삶’이라고 부를 수 있을까?",
                        "공장은 멈췄고 숲은 시들었으며, 치안이 사라진 자리에서 사람들은 웃는 법을 잊었다.",
                        "거대한 폭발도, 화려한 종말도 없었다. 모든 것은 그저… 아주 조용히 멈춰갔다.",
                        "꺼지기 직전의 촛불이 내는 그 희미한 빛… 지금의 우리가 바로 그것이다.",
                        "내일도 해는 뜨겠지만, 그 빛을 맞이할 사람은 아무도 없을 것이다."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "We survived. But can we really call this 'life'?",
                        "Factories stopped, forests withered, and in the place where security vanished, people forgot how to smile.",
                        "There was no massive explosion, no spectacular apocalypse. Everything just... quietly came to a halt.",
                        "The faint light of a candle just before it goes out... that is what we are now.",
                        "The sun will rise tomorrow, but there will be no one to greet its light."
                    }
                }
            }
        },
        {
            "ENDING_01_MASKED_TYCOON", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "창밖을 봐. 저 회색 스모그 너머에 무너져가는 빈민가가 보이지?",
                        "우린 이 돔 안에서, 고급 와인을 마시며 영원히 안전할 거야.",
                        "숨 쉬는 공기조차 돈으로 사야 하는 세상이 왔지만… 뭐 어때? 우린 승자잖아.",
                        "폐가 조금 무거운 느낌이 들긴 하지만, 통장 잔고를 보면 다시 숨이 쉬어진다.",
                        "자, 건배할까? 썩어가는 지구와… 그 위에서 영원할 우리의 부에."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "Look outside the window. Do you see the crumbling slums beyond that gray smog?",
                        "Inside this dome, we will sip fine wine and be safe forever.",
                        "A world has come where even the air we breathe must be bought... but so what? We are the winners.",
                        "My lungs feel a bit heavy, but looking at my bank balance helps me breathe again.",
                        "Shall we toast? To the rotting earth... and to our eternal wealth upon it."
                    }
                }
            }
        },
        {
            "ENDING_02_RETURN_FOREST", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "저 녹슨 자동차 위에 핀 꽃 좀 봐.",
                        "사람들은 평생을 바쳐 저걸 사려고 했었지. 지금은 그냥 고철 더미인데.",
                        "전기는 끊겼고 시장엔 먼지만 가득하지만, 밤하늘의 은하는 그 어느 때보다 선명하다.",
                        "우리는 사냥으로 배를 채우고, 서로의 체온으로 추위를 견딘다.",
                        "문명이 멸망한 게 아니다. 우리는 그저 진짜 ‘자연’의 일부로 돌아왔을 뿐이다."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "Look at the flower blooming on that rusty car.",
                        "People used to spend their whole lives trying to buy that. Now it's just a pile of scrap metal.",
                        "The electricity is cut and the market is full of dust, but the galaxy in the night sky is clearer than ever.",
                        "We fill our bellies by hunting and endure the cold with each other's body heat.",
                        "Civilization didn't perish. We just returned to being part of true 'nature'."
                    }
                }
            }
        },
        {
            "ENDING_03_FORBIDDEN_CITY", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "시민 여러분, 안심하십시오. 중앙 통제 시스템은 여러분의 심박수까지 완벽히 관리하고 있습니다.",
                        "불필요한 웃음과 눈물은 사회 질서를 교란하는 바이러스입니다.",
                        "이웃이 수상한 감정을 보인다면 즉시 신고하십시오. 그것이 애국입니다.",
                        "우리는 기계처럼 정밀하고, 강철처럼 단단한 완벽한 사회를 완성했습니다.",
                        "자유는 혼란만 낳습니다. 생각하지 마십시오. 그저 복종하세요."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "Citizens, rest assured. The central control system perfectly manages even your heart rate.",
                        "Unnecessary laughter and tears are viruses that disturb the social order.",
                        "Report immediately if your neighbor shows suspicious emotions. That is patriotism.",
                        "We have completed a perfect society, precise as a machine and hard as steel.",
                        "Freedom only breeds chaos. Do not think. Just obey."
                    }
                }
            }
        },
        {
            "ENDING_04_LAWLESS_PLEASURE", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "이이야아!! 음악 더 크게 틀어!! 오늘이 세상의 마지막 날인 것처럼 놀자고!!",
                        "경찰? 법? 그런 따분한 쓰레긴 어제 다 태워버렸어!",
                        "우리가 법이야! 내일 굶어 죽든, 칼 맞아 죽든 누가 신경 써? 난 지금 최고라고!",
                        "약탈한 술이랑 고기 더 가져와! 이제 막 시작이잖아!",
                        "멈추는 놈은 배신자다! 마시고, 즐기고, 미쳐버려!!"
                    }
                },
                {
                    Language.English, new[]
                    {
                        "Yee-haw!! Turn up the music!! Let's party like it's the end of the world!!",
                        "Police? Law? We burned that boring trash yesterday!",
                        "We are the law! Who cares if we starve or get stabbed tomorrow? I feel great right now!",
                        "Bring more looted booze and meat! We're just getting started!",
                        "Anyone who stops is a traitor! Drink, enjoy, and go crazy!!"
                    }
                }
            }
        },
        {
            "ENDING_05_COLD_STEEL", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "이 도시는 거대한 공장이고, 너는 언제든 교체 가능한 부품일 뿐이다.",
                        "비효율적인 인간에게 배급표는 없다.",
                        "하늘 보며 감상에 젖지 말고, 나사 하나 더 조여.",
                        "그것이 네 존재 가치다. 기업의 이익이 곧 국가의 이익이니까.",
                        "휴식 시간은 끝났다. 사이렌이 울린다—죽을 때까지 다시 일해라."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "This city is a giant factory, and you are just a replaceable part.",
                        "There are no ration tickets for inefficient humans.",
                        "Don't get sentimental looking at the sky, just tighten one more screw.",
                        "That is your value of existence. Because the company's profit is the nation's profit.",
                        "Break time is over. The siren is ringing—get back to work until you die."
                    }
                }
            }
        },
        {
            "ENDING_06_SLUM_UTOPIA", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "오늘 저녁도 가진 게 없으니 풀죽이나 끓여 먹겠지.",
                        "그런데 이상하지 않아? 돈은 한 푼도 없는데 마음은 이렇게 평온해.",
                        "맑은 공기 마시고, 기타 치고, 노래 부르며… 난 부자들이 부럽지 않아.",
                        "내일 무슨 일이 일어날진 모르지만, 적어도 지금 이 순간만큼은 자유야.",
                        "그럼 된 거 아니야? 자, 걱정은 내려놓고 계속 노래하자."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "I guess we're eating grass porridge again tonight since we have nothing.",
                        "But isn't it strange? I don't have a penny, yet my heart is so peaceful.",
                        "Breathing fresh air, playing guitar, singing... I don't envy the rich.",
                        "I don't know what will happen tomorrow, but at least for this moment, I am free.",
                        "Isn't that enough? Come on, put your worries down and let's keep singing."
                    }
                }
            }
        },
        {
            "ENDING_10_GLASS_GARDEN", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "푸른 온실 안에서 사계절은 사라지고, 꽃은 인공 토양에서 자란다.",
                        "공기는 깨끗하고 생산 라인은 멈추지 않는다. 적어도 굶는 사람은 없다.",
                        "하지만 사람들은 서로를 믿지 않는다. 미소 대신 데이터가 오간다.",
                        "이곳은 번영했지만, 마음은 메말라버렸다.",
                        "우리는 풍요를 얻고, 온기를 잃었다.",
                        "식물은 잘 자라는데… 왜 인간성은 시들어가는 걸까?"
                    }
                },
                {
                    Language.English, new[]
                    {
                        "Inside the blue greenhouse, the four seasons are gone, and flowers grow in artificial soil.",
                        "The air is clean and the production lines never stop. At least no one is starving.",
                        "But people don't trust each other. Data is exchanged instead of smiles.",
                        "This place has prospered, but hearts have dried up.",
                        "We gained abundance, but lost warmth.",
                        "The plants grow well... so why is humanity withering away?"
                    }
                }
            }
        },
        {
            "ENDING_09_GOLDEN_DESERT", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "모래폭풍이 유리 벽을 긁는 소리를 들어봐.",
                        "이 세상엔 숲도 강도 없지만, 금고는 가득 차 있다.",
                        "땅은 죽었고, 기업은 살아남았다. 화폐는 늘었지만 사람은 줄었다.",
                        "우리는 부자가 되었지만, 뿌리내릴 땅을 잃었다.",
                        "언젠가 다음 세대는 묻게 될 것이다. 왜 우리에게 남겨진 건 황금과 폐허뿐이냐고."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "Listen to the sound of the sandstorm scratching the glass walls.",
                        "There are no forests or rivers in this world, but the vaults are full.",
                        "The land is dead, but the corporations survived. Currency increased, but people decreased.",
                        "We became rich, but lost the land to put down roots.",
                        "Someday, the next generation will ask: Why is all we have left gold and ruins?"
                    }
                }
            }
        },
        {
            "ENDING_11_BLUE_PRISON", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "거리는 조용하고 범죄율은 제로다.",
                        "아무도 싸우지 않고, 아무도 저항하지 않는다.",
                        "하지만 밤마다 창밖을 스캔하는 드론을 보면 심장이 얼어붙는다.",
                        "이곳을 지배하는 건 평화가 아니라 침묵이다.",
                        "폭동은 사라졌고, 우리의 목소리도 함께 묻혔다.",
                        "눈에 보이는 쇠창살은 없지만… 이 도시 전체가 감옥이다."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "The streets are quiet and the crime rate is zero.",
                        "No one fights, no one resists.",
                        "But my heart freezes when I see drones scanning outside the window every night.",
                        "What rules this place is not peace, but silence.",
                        " riots are gone, and our voices are buried with them.",
                        "There are no visible iron bars... but this entire city is a prison."
                    }
                }
            }
        },
        {
            "ENDING_12_TWILIGHT_SMILES", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "불빛 하나 없는 골목에서도 기타 소리는 멈추지 않는다.",
                        "가난한 아이들은 서로의 이름을 부르며 환하게 웃는다.",
                        "유리탑은 무너졌지만, 우리의 마음은 오히려 더 높이 올라갔다.",
                        "법은 약하고 밤은 위험하지만, 희망은 이상할 만큼 크다.",
                        "이 연약한 낙원에서 우리는 진심으로 웃고 있다.",
                        "폭풍이 오면 무너질지도 모른다… 그래도, 우리는 정말 행복했다."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "Even in the alley without a single light, the guitar sound never stops.",
                        "Poor children call each other's names and smile brightly.",
                        "The glass towers collapsed, but our hearts soared even higher.",
                        "The law is weak and the night is dangerous, but hope is strangely large.",
                        "In this fragile paradise, we are genuinely smiling.",
                        "It might collapse when the storm comes... but still, we were truly happy."
                    }
                }
            }
        },
        {
            "ENDING_07_PRECARIOUS_PEACE", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "완벽한 천국은 아니지만, 기적처럼 지옥으로 떨어지는 건 막아냈다.",
                        "누군가는 돈이 더 필요하다고 불평하고, 누군가는 자유가 부족하다고 말한다.",
                        "그럼에도 우리는 어느 한쪽으로 기울지 않는 위태로운 줄타기에 성공했다.",
                        "폭동도, 대기근도, 숨 막히는 독재도 없는… 그저 평범하고 지루한 하루.",
                        "어쩌면 우리가 이토록 치열하게 싸워온 이유는… 이 ‘평범함’을 지키기 위해서였을지도 모른다."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "It's not a perfect heaven, but we miraculously stopped it from falling into hell.",
                        "Some complain they need more money, others say there isn't enough freedom.",
                        "Nevertheless, we succeeded in a precarious tightrope walk that doesn't lean to either side.",
                        "No riots, no great famine, no suffocating dictatorship... just an ordinary, boring day.",
                        "Perhaps the reason we fought so fiercely... was to protect this 'ordinariness'."
                    }
                }
            }
        },
        {
            "ENDING_13_COLORLESS_MID", new()
            {
                {
                    Language.Korean, new[]
                    {
                        "극단은 피했지만, 이상에는 닿지 못했다.",
                        "이곳은 희미한 회색의 세계—꿈도 악몽도 아니다.",
                        "거리에서 굶어 죽는 사람은 없지만, 축제가 열리지도 않는다.",
                        "모든 것이 조금 부족하고, 조금 남아 있다.",
                        "우리는 살아남았다. 하지만 정말 ‘살고 있다’고 말하긴 어렵다.",
                        "그래도… 어쩌면 아직, 새로운 선택을 할 여지는 남아 있는지도 모른다."
                    }
                },
                {
                    Language.English, new[]
                    {
                        "We avoided the extremes, but didn't reach the ideal.",
                        "This is a faint gray world—neither a dream nor a nightmare.",
                        "No one is starving to death on the streets, but no festivals are held either.",
                        "Everything is a little lacking, and a little left over.",
                        "We survived. But it's hard to say we are really 'living'.",
                        "Still... maybe, just maybe, there is room left to make a new choice."
                    }
                }
            }
        }
    };
}