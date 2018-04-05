using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager {

    public Dictionary<int, NetworkEntity> netEntities;
    private int lastEntityId;

    public EntityManager() {
        lastEntityId = 0;
        netEntities = new Dictionary<int, NetworkEntity>();
    }

    public int RegisterEntity(GameObject entity) {
        NetworkEntity netEntity = entity.GetComponent<NetworkEntity>();
        if (netEntity == null) {
            return -1;
        }
        lastEntityId++;
        netEntity.EntityID = lastEntityId;
        netEntities.Add(lastEntityId, netEntity.GetComponent<NetworkEntity>());
        return lastEntityId;
    }

    public GameObject createEntity(GameObject prefab, Vector3 pos, Quaternion rot, byte objId, out int id) {
        GameObject newEntity = GameObject.Instantiate(prefab, pos, rot);
       
        NetworkEntity netEntity = newEntity.GetComponent<NetworkEntity>();
        if (netEntity == null) {
            id = -1;
            return null;
        }
        lastEntityId++;
        netEntity.EntityID = lastEntityId;
        netEntity.ObjectType = objId; //proffing?
        netEntities.Add(lastEntityId, newEntity.GetComponent<NetworkEntity>());

        id = lastEntityId;
        return newEntity;
    }

    public void RemoveEntity(int idToRemove) {
        netEntities.Remove(idToRemove);
    }


}
