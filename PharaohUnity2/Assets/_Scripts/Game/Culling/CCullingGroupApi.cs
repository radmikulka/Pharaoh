// =================================
// AUTHOR:		Radek Mikulka
// DATE			2019-09-25
// =================================

using UnityEngine;
using System;
using System.Collections.Generic;
using AldaEngine.AldaFramework;
using Zenject;

namespace TycoonBuilder
{
    public class CCullingGroupApi : MonoBehaviour, IInitializable
    {
       private class CCullingGroup
       {
          public BoundingSphere BoundingSphere;
          public int Index;
          public readonly IIHaveCullingGroup Group;

          public CCullingGroup(BoundingSphere pBoundingSphere, int pIndex, IIHaveCullingGroup pGroup)
          {
             BoundingSphere = pBoundingSphere;
             Index = pIndex;
             Group = pGroup;
          }
       }

       private const int MaxItemsCount = 500;

       private readonly Dictionary<IIHaveCullingGroup, CCullingGroup> _db = new();
       private readonly Dictionary<int, CCullingGroup> _dbByIndexes = new(); 
       private readonly BoundingSphere[] _spheres = new BoundingSphere[MaxItemsCount];

       private IMainCameraProvider _mainCameraProvider;
       private CullingGroup _cullingGroup;
       
       private int _activeCount;

       [Inject]
       private void Inject(IMainCameraProvider mainCameraProvider)
       {
          _mainCameraProvider = mainCameraProvider;
       }

       public void Initialize()
       {
          _cullingGroup = new CullingGroup();
          
          _cullingGroup.SetBoundingSpheres(_spheres);
          _cullingGroup.SetBoundingSphereCount(0);
          _cullingGroup.targetCamera = _mainCameraProvider.Camera.Camera;
          _cullingGroup.onStateChanged += OnStateChanged;
       }

       public bool IsVisible(IIHaveCullingGroup haveCullingGroup)
       {
          return _db.TryGetValue(haveCullingGroup, out CCullingGroup group) && _cullingGroup.IsVisible(group.Index);
       }

       public void RegisterCullingGroup(IIHaveCullingGroup owner)
       {
          if (_activeCount >= MaxItemsCount)
          {
             Debug.LogError("No free culling groups indexes left");
             return;
          }
          
          if(_db.ContainsKey(owner))
             return;
          
          int index = _activeCount;
          
          BoundingSphere newSphere = new(owner.Position, owner.Radius);
          CCullingGroup wrapper = new(newSphere, index, owner);

          _db.Add(owner, wrapper);
          _dbByIndexes.Add(index, wrapper);
          _spheres[index] = newSphere;

          _activeCount++;
          _cullingGroup?.SetBoundingSphereCount(_activeCount);
       }

       public void UnregisterCullingGroup(IIHaveCullingGroup owner)
       {
          if (!_db.TryGetValue(owner, out CCullingGroup groupToRemove)) 
             return;
          
          int indexToRemove = groupToRemove.Index;
          int lastIndex = _activeCount - 1;

          if (indexToRemove != lastIndex)
          {
             CCullingGroup lastGroup = _dbByIndexes[lastIndex];
             
             _spheres[indexToRemove] = _spheres[lastIndex];
             lastGroup.Index = indexToRemove;
             lastGroup.BoundingSphere = _spheres[indexToRemove]; 
             
             _dbByIndexes[indexToRemove] = lastGroup;
          }
          
          _dbByIndexes.Remove(lastIndex);
          _db.Remove(owner);

          _activeCount--;
          _cullingGroup.SetBoundingSphereCount(_activeCount);
       }

       private void OnStateChanged(CullingGroupEvent pSphere)
       {
          if (_dbByIndexes.TryGetValue(pSphere.index, out CCullingGroup group))
          {
             group.Group.OnStateChange(pSphere.isVisible);
          }
       }

       private void LateUpdate()
       {
          foreach (var record in _db)
          {
             IIHaveCullingGroup owner = record.Key;
             CCullingGroup wrapper = record.Value;

             if (!owner.UpdatePosition) 
                continue;
             wrapper.BoundingSphere.position = owner.Position;
             _spheres[wrapper.Index] = wrapper.BoundingSphere;
          }
       }

       private void OnDestroy()
       {
          _cullingGroup?.Dispose();
          _cullingGroup = null;
       }
    }
}