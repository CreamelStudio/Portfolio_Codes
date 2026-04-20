using GlobalAudio;
using Island;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;
using static WorldInputManager;

public partial class CharacterController
{
    [Header("Place Block")]
    [SerializeField] private Vector3 placeBlockPosition;            // 블록 설치 위치

    [Header("Combat")]
    [SerializeField] private float attackRange;                     // 공격 사거리
    [SerializeField] private LayerMask enemyLayers;                 // 적 레이어 마스크

    #region 딜레이 관련
    private bool isDelaying = false;
    private Coroutine delayCoroutine;

    public void StartDelay()
    {
        if (!isDelaying)
        {
            delayCoroutine = StartCoroutine(DelayCoroutine());
        }
    }

    public bool IsDelaying()
    {
        return isDelaying;
    }

    public void StopDelay()
    {
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
            isDelaying = false;
        }
    }

    private IEnumerator DelayCoroutine()
    {
        isDelaying = true;
        yield return new WaitForSeconds(0.4f);
        isDelaying = false;
        delayCoroutine = null;
    }
    #endregion

    /// <summary>
    /// 좌클릭 입력 처리
    /// </summary>
    private void LeftClickEvent()
    {
        if (isInWater || IsDelaying()) return;


        if (Mouse.current.leftButton.IsPressed())
        {
            // 좌클릭 기본 액션
            if (isFirstPerson && currentToolType != ToolItemType.Fishingrod)
            {
                int? animHash = CharacterAnimatorParamMapper.GetAnimationTrigger(currentToolType);
                firstPersonController.PlayFpItemSwingAnimation();

                if (animHash.HasValue)
                {
                    if (LOPNetworkManager.Instance.isConnected) LOPNetworkManager.Instance.SendAnimationTrigger(networkIdentity.NetworkId, animHash.Value);
                }
            }
            else
            {
                if (currentToolType == ToolItemType.Fishingrod && highLightBlock != null)
                {
                    Vector3Int blockAbove = Vector3Int.FloorToInt(highLightBlock.position) + Vector3Int.up;
                    if (map.GetBlockInChunk(blockAbove + Vector3.down, ChunkType.Water).id != BlockConstants.Water) return;
                }
                else if (currentToolType == ToolItemType.Fishingrod && highLightBlock == null) return;
                int? animHash = CharacterAnimatorParamMapper.GetAnimationTrigger(currentToolType);
                if (animHash.HasValue)
                {
                    if (LOPNetworkManager.Instance.isConnected)
                    {
                        anim.SetTrigger(animHash.Value);
                        LOPNetworkManager.Instance.SendAnimationTrigger(networkIdentity.NetworkId, animHash.Value);
                    }
                    else
                    {
                        anim.SetTrigger(animHash.Value);
                    }
                }
            }
            if (isFirstPerson) TimingAnimation();
            else if (currentToolType != ToolItemType.None) QuickslotNumberBtn.Instance.cantChange = true;
            StartDelay();
        }
    }

    public void TimingAnimation()
    {
        if (LOPNetworkManager.Instance.isConnected && !networkIdentity.IsOwner) return;
        if (QuickslotNumberBtn.Instance.cantChange && currentToolType != ToolItemType.Fishingrod) QuickslotNumberBtn.Instance.cantChange = false;
        Ray ray = new Ray(GetRayOrigin().position, GetRayOrigin().forward);

        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, entityLayer))
        {
            AudioPlayer.PlaySound(penguinAudioObject, swordSFX, 1f);
            if (hit.transform.gameObject.CompareTag("Player") && hit.transform.gameObject != this)
            {
                CharacterController character = hit.transform.GetComponent<CharacterController>();
                character.TakeDamage(2);
            }
            else
            {
                Monster monster = hit.transform.GetComponent<Monster>();
                if (monster != null && !monster.IsDead)
                {
                    if (currentToolType == ToolItemType.Sword) monster.TakeDamage(characterStat.atk); //테스트 용으로 잠시 Axe로 사용
                    else monster.TakeDamage(5);
                }
                return;
            }
        }
        if (highLightBlock == null) return;


        Vector3Int blockAbove = Vector3Int.FloorToInt(highLightBlock.position) + Vector3Int.up;
        string blockID = map?.GetBlockInChunk(blockAbove, ChunkType.Ground).id;
        Debug.Log(blockID);

        switch (currentToolType)
        {
            case ToolItemType.None:
                {
                    break;
                }
            case ToolItemType.Axe:
                {
                    // 나무 채집
                    if (blockID == BlockConstants.Trees)
                    {
                        //Collider[] col = Physics.OverlapBox(highlightBlockPos, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, treeLayerMask);
                        //foreach (Collider c in col)
                        //{
                        //    TreeChop treeChop = c.GetComponent<TreeChop>();

                        //    if (LOPNetworkManager.Instance.isConnected)
                        //    {
                        //        LOPNetworkManager.Instance.RPC(treeChop, "ChopingTree");
                        //    }
                        //    else
                        //    {
                        //        treeChop.ChopingTree();
                        //    }

                        //    AudioPlayer.PlaySound(penguinAudioObject, axeSFX, 1f);
                        //    break;
                        //}
                        GameObject block = highLightBlockObject.GetComponent<DesrtoyBlock>().other;
                        TreeChop treeChop = block.GetComponent<TreeChop>();
                        if (LOPNetworkManager.Instance.isConnected)
                        {
                            LOPNetworkManager.Instance.RPC(treeChop, "ChopingTree");
                        }
                        else
                        {
                            treeChop.ChopingTree();
                        }

                        AudioPlayer.PlaySound(penguinAudioObject, axeSFX, 1f);
                        break;
                    }

                    break;
                }
            case ToolItemType.Spoon:
                {
                    // 흙, 눈
                    if (blockID == BlockConstants.Ground || blockID == BlockConstants.TilledSoil || blockID == BlockConstants.WetTilledSoil)
                    {
                        Vector3Int blockAbove2 = blockAbove + Vector3Int.up;
                        string blockID2 = map?.GetBlockInChunk(blockAbove2, ChunkType.Ground).id;
                        if (blockID2 == BlockConstants.Crops)
                        {
                            break;
                        }
                        AudioPlayer.PlaySound(penguinAudioObject, soilBreakSFX, 1f);

                        DestroyBlock(blockAbove);

                    }
                    else if (blockID == BlockConstants.Snow)
                    {
                        AudioPlayer.PlaySound(penguinAudioObject, snowBreakSFX, 1f);

                        DestroyBlock(blockAbove);
                    }
                    break;
                }
            case ToolItemType.Hammer:
                {
                    // 돌, 광석, 얼음

                    if (blockID == BlockConstants.Stone
                        || blockID == BlockConstants.GoldOre
                        || blockID == BlockConstants.SilverOre)
                    {
                        AudioPlayer.PlaySound(penguinAudioObject, stoneBreakSFX, 1f);

                        DestroyBlock(blockAbove);
                    }
                    else if (blockID == BlockConstants.Ice)
                    {
                        AudioPlayer.PlaySound(penguinAudioObject, hammerSFX, 1f);

                        DestroyBlock(blockAbove);
                    }
                    break;
                }
            case ToolItemType.Fishingrod:
                {
                    if (map.GetBlockInChunk(blockAbove + Vector3.down, ChunkType.Water).id == BlockConstants.Water && !isFishing && isGrounded)
                    {
                        wasFirstPersonBeforeFishing = isFirstPerson;

                        if (wasFirstPersonBeforeFishing) TogglePerspective();
                        rb.linearVelocity = Vector3.zero;
                        isFishing = true;
                        int? animHash = CharacterAnimatorParamMapper.GetAnimationTrigger(currentToolType);
                        if (animHash.HasValue)
                        {
                            anim.SetTrigger(animHash.Value);
                        }
                        anim.SetTrigger(CharacterAnimatorParamMapper.CanFishing);
                        FishingManager.Instance?.FishingStart(this);
                        WorldInputManager.Instance.gameInputType = GameInputType.isOpenFishing;
                    }
                    else
                    {
                        Debug.Log($"낚시를 시작할 수 없습니다. waterBlock={map.GetBlockInChunk(blockAbove + Vector3.down, ChunkType.Water).id}, isFishing={isFishing}");
                    }
                    break;
                }
            case ToolItemType.Folk:
                {

                    if (blockID == BlockConstants.Ground)
                    {
                        map.Soil(blockAbove);
                    }
                    break;
                }
        }
        if (QuickslotNumberBtn.Instance.selectedItem.item != null)
        {
            if (QuickslotNumberBtn.Instance.selectedItem.item.metaType == ItemMetaType.Seed && QuickslotNumberBtn.Instance.selectedItem.amount > 0)
            {
                string itemID = QuickslotNumberBtn.Instance.selectedItem.item.itemID;
                CropManager.Instance.PlantCropOnMap(itemID, blockAbove);
                characterUIController.GaugeUpdate();
                return;
            }
        }

    }

    /// <summary>
    /// 우클릭 입력 처리
    /// </summary>
    private void RightClickEvent()
    {
        if (isInWater) return;
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            GameObject block = highLightBlockObject.GetComponent<DesrtoyBlock>().other;
            if (block != null)
            {
                Crop crop = block.GetComponent<Crop>();
                if (crop != null)
                {
                    Debug.Log($"작물 수확: {block.gameObject.name}");

                    crop.Harvest();
                    //block = highLightBlockObject.GetComponent<DesrtoyBlock>().other = null;
                    return;
                }
            }

            Vector3Int blockAbove = Vector3Int.zero;
            if (highLightBlock != null)
            {
                blockAbove = Vector3Int.FloorToInt(highLightBlock.position) + Vector3Int.up;
                string blockID = map?.GetBlockInChunk(blockAbove, ChunkType.Ground).id;

                if (blockID == BlockConstants.TilledSoil || blockID == BlockConstants.WetTilledSoil)
                {

                }
            }


            if (QuickslotNumberBtn.Instance.selectedItem.item != null)
            {
                if (QuickslotNumberBtn.Instance.selectedItem.item.toolType == ToolItemType.parka)
                {
                    QuickslotNumberBtn.Instance.WearableParka = true;
                    QuickslotNumberBtn.Instance.inventorySlots[QuickslotNumberBtn.Instance.currentSlotIndex].currentItem = null;
                    QuickslotNumberBtn.Instance.inventorySlots[QuickslotNumberBtn.Instance.currentSlotIndex].UpdateUI();
                    QuickslotNumberBtn.Instance.OnWearableParka?.Invoke();
                    return;
                }
                else if (QuickslotNumberBtn.Instance.selectedItem.item.metaType == ItemMetaType.Food)
                {
                    InventoryManager.Instance.RemoveItem(new InventoryItem(QuickslotNumberBtn.Instance.selectedItem.item, 1));
                    if (QuickslotNumberBtn.Instance.selectedItem.item.metaData is FoodItem food)
                    {
                        AudioPlayer.PlaySound(penguinAudioObject, eatingSFX, 1f);
                        characterStat.curHunger += food.hungerRestore;
                        characterStat.curHp += food.healAmount;
                        characterUIController.GaugeUpdate();
                        return;
                    }
                }

                else if (QuickslotNumberBtn.Instance.selectedItem.item.metaType == ItemMetaType.WaterBottle && highLightBlock != null)
                {
                    map.WetSoil(blockAbove, 2);
                    return;
                }

                else if (QuickslotNumberBtn.Instance.selectedItem.item.metaType == ItemMetaType.Seed)
                {
                    InventoryManager.Instance.RemoveItem(new InventoryItem(QuickslotNumberBtn.Instance.selectedItem.item, 1));
                    if (QuickslotNumberBtn.Instance.selectedItem.item.metaData is FoodItem food)
                    {
                        AudioPlayer.PlaySound(penguinAudioObject, eatingSFX, 1f);
                        characterStat.curHunger += food.hungerRestore;
                        characterStat.curHp += food.healAmount;
                        characterUIController.GaugeUpdate();
                        return;
                    }
                }

                if (isFirstPerson && currentToolType != ToolItemType.Fishingrod)
                {
                    firstPersonController.PlayFpItemSwingAnimation();
                }
                else if (currentToolType == ToolItemType.None)
                {
                    int? animHash = CharacterAnimatorParamMapper.GetAnimationTrigger(currentToolType);
                    if (animHash.HasValue)
                    {
                        anim.SetTrigger(animHash.Value);
                    }
                }
            }


            PlaceBlockEvent();
        }
    }

    /// <summary>
    /// 블록 설치 처리
    /// </summary>
    private void PlaceBlockEvent()
    {
        InventoryItem selectedItem = QuickslotNumberBtn.Instance.selectedItem;
        if (selectedItem != null && selectedItem.item != null && selectedItem.item.metaType == ItemMetaType.Block)
        {
            if (selectedItem.item.metaData is BlockItem block && InventoryManager.Instance.isHandleItem(new InventoryItem(selectedItem.item, 1)))
            {
                bool blockHit = Physics.BoxCast(placeBlockPosition + Vector3.down * 2.2f, highLightBlock.localScale / 2, Vector3.up, out RaycastHit hit, highLightBlock.rotation, 2f, entityLayer);

                if (blockHit)
                {
                    Debug.Log("블록을 설치할 수 없습니다. 플레이어가 설치 위치에 너무 가깝습니다.");
                    return;
                }

                AudioPlayer.PlaySound(penguinAudioObject, blockInsSFX, 1f);

                Vector3Int blockAbove = Vector3Int.FloorToInt(highLightBlock.position);
                string blockID3 = map.GetBlockInChunk(blockAbove + Vector3Int.up, ChunkType.Water).id;
                Debug.Log($"blockAboveblockAbove {blockID3}");
                if (blockID3 == BlockConstants.Bedrock)
                {
                    if (LOPNetworkManager.Instance.isConnected)
                    {
                        blockAbove = Vector3Int.FloorToInt(highLightBlock.position) + Vector3Int.up;
                        LOPNetworkManager.Instance.SendBlockUpdate(blockAbove, block.placedBlockId, 0);
                    }
                    else
                    {
                        map.GetChunkFromPosition(placeBlockPosition, ChunkType.Ground).EditVoxel(placeBlockPosition + Vector3.up, MapSettingManager.Instance.Map.FindBlockType(block.placedBlockId));
                        string blockID2 = map.GetBlockInChunk(placeBlockPosition, ChunkType.Water).id;
                    }
                }

                string blockID = map.GetBlockInChunk(blockAbove, ChunkType.Water).id;

                if (blockID != null)
                {
                    if (blockID == BlockConstants.Water)
                    {
                        if (LOPNetworkManager.Instance.isConnected)
                        {

                            LOPNetworkManager.Instance.SendBlockUpdate(blockAbove + Vector3Int.up, block.placedBlockId, 0);
                            LOPNetworkManager.Instance.SendBlockUpdate(blockAbove, block.placedBlockId, 0);
                            LOPNetworkManager.Instance.SendBlockUpdate(blockAbove + Vector3Int.down, block.placedBlockId);
                        }
                        else
                        {
                            map.GetChunkFromPosition(placeBlockPosition, ChunkType.Ground).EditVoxel(blockAbove + Vector3Int.up, MapSettingManager.Instance.Map.FindBlockType(block.placedBlockId));
                            map.GetBlockInChunk(blockAbove + Vector3Int.down, ChunkType.Water).id = block.placedBlockId;
                        }
                    }
                    else
                    {
                        if (LOPNetworkManager.Instance.isConnected)
                        {
                            blockAbove = Vector3Int.FloorToInt(placeBlockPosition);
                            LOPNetworkManager.Instance.SendBlockUpdate(blockAbove + Vector3Int.up, block.placedBlockId, 0);
                            string blockID2 = map.GetBlockInChunk(blockAbove, ChunkType.Water).id;

                            if (blockID2 == BlockConstants.Water)
                            {
                                LOPNetworkManager.Instance.SendBlockUpdate(blockAbove, block.placedBlockId);
                                LOPNetworkManager.Instance.SendBlockUpdate(blockAbove + Vector3Int.down, block.placedBlockId);
                            }
                        }
                        else
                        {
                            map.GetChunkFromPosition(placeBlockPosition, ChunkType.Ground).EditVoxel(placeBlockPosition + Vector3.up, MapSettingManager.Instance.Map.FindBlockType(block.placedBlockId));
                            string blockID2 = map.GetBlockInChunk(placeBlockPosition, ChunkType.Water).id;
                            if (blockID2 == BlockConstants.Water)
                            {
                                map.GetBlockInChunk(placeBlockPosition, ChunkType.Water).id = block.placedBlockId;
                                map.GetBlockInChunk(placeBlockPosition + Vector3Int.down, ChunkType.Water).id = block.placedBlockId;
                            }

                        }
                    }
                    InventoryManager.Instance.RemoveItem(new InventoryItem(selectedItem.item, 1));
                }
            }
        }

    }
}
