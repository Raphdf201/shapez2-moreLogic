using ShapezShifter.Kit;
using UnityEngine;

namespace MoreLogic;

public class NAndGateDrawData : INAndGateDrawData
{
    public static BuildingDrawData CreateCubeDrawData()
    {
        Mesh baseMesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().sharedMesh;;

        LOD6Mesh baseModLod = MeshLod.Create().AddLod0Mesh(baseMesh).BuildLod6Mesh();

        return new BuildingDrawData(
            renderVoidBelow: false,
            [baseModLod, baseModLod, baseModLod],
            baseModLod,
            baseModLod,
            baseModLod.LODClose,
            new LODEmptyMesh(),
            BoundingBoxHelper.CreateBasicCollider(baseMesh),
            new NAndGateDrawData(),
            false,
            null,
            false);
    }
    
    public static BuildingDrawData CreateDrawData()
    {
        string baseMeshPath = Main.Res.SubPath("nand.fbx");
        Mesh baseMesh = FileMeshLoader.LoadSingleMeshFromFile(baseMeshPath);
        LOD6Mesh lodMesh = MeshLod.Create().AddLod0Mesh(baseMesh)
            .UseLod0AsLod1()
            .UseLod1AsLod2()
            .UseLod2AsLod3()
            .UseLod3AsLod4()
            .UseLod4AsLod5()
            .BuildLod6Mesh();

        return new BuildingDrawData(
            renderVoidBelow: false,
            [lodMesh, lodMesh, lodMesh],
            lodMesh,
            lodMesh,
            lodMesh.LODClose,
            new LODEmptyMesh(),
            BoundingBoxHelper.CreateBasicCollider(baseMesh),
            new NAndGateDrawData(),
            false,
            null,
            false);
    }
}
