using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]

[Serializable]
public class MaleClothingSkins
{
	public Material[] Skins;
}

public class MannequinMale : MonoBehaviour
{
	public static int TopType { get; set; }
	public static int BottomType { get; set; }
	public static int TieType { get; set; }
	public static int TopSkin { get; set; }
	public static int BottomSkin { get; set; }
	public static int MannequinType { get; set; }

	public GameObject mannequinMesh;

	public GameObject topMesh;
	public GameObject bottomMesh;
	public GameObject tieMesh;

	public Material[]	mannequinTopMaskMaterials;
	public Material[]	mannequinBottomMaskMaterials;
	public Material		mannequinTopMaskOff;
	public Material		mannequinBottomOff;

	public MeshFilter[]	mannequinMeshes;
	public MeshFilter[] topMeshes;
	public MeshFilter[] bottomMeshes;
	public MeshFilter[] tieMeshes;

	public MeshRenderer[]	topMeshRenderers;
	public MeshRenderer[]	mannequinMeshRenders;

	public MaleClothingSkins[]	topSkins;
	public MaleClothingSkins[]	bottomSkins;

	public bool bUsesMannequinMasks = true;

	[SerializeField]
    public int sTopType = 0; // Developer set value in the inspector, for testing purpose
	[SerializeField]
	private int sBottomType = 0;  
	[SerializeField]
	private int sTieType = 0;  
	[SerializeField]
	private int sTopSkin = 0; 
	[SerializeField]
	private int sBottomSkin = 0;
	[SerializeField]
	private int sMannequinType = 0; 

	
    void Start()
	{
		if (!Application.isEditor)
		{
			MannequinType = TopType = BottomType = TopSkin = BottomSkin = TieType = 0; // Normal value, used for the final game
		}
	}

	void OnValidate()
	{
		//if ( sTopType < -1 )
		//	sTopType = -1;
		//else if  ( sTopType > topMeshes.Length - 1 )
		//	sTopType = topMeshes.Length - 1;

		if ( sBottomType < -1 )
			sBottomType = -1;
		else if  ( sBottomType > bottomMeshes.Length - 1 )
			sBottomType = bottomMeshes.Length - 1;

		if ( sTieType < -1 )
			sTieType = -1;
		else if  ( sTieType > tieMeshes.Length - 1 )
			sTieType = tieMeshes.Length - 1;
		

		if ( sTopType >= 0 )
		{
			if ( sTopSkin < 0 )
				sTopSkin = 0;
			else if  ( sTopSkin > topSkins[ sTopType ].Skins.Length - 1 )
				sTopSkin = topSkins[ sTopType ].Skins.Length - 1;
		}

		if ( sBottomType >= 0 )
		{
			if ( sBottomSkin < 0 )
				sBottomSkin = 0;
			else if  ( sBottomSkin > bottomSkins[ sBottomType ].Skins.Length - 1 )
				sBottomSkin = bottomSkins[ sBottomType ].Skins.Length - 1;
		}

		if ( sMannequinType < 0 )
			sMannequinType = 0;
		else if  ( sMannequinType > mannequinMeshes.Length - 1 )
			sMannequinType = mannequinMeshes.Length - 1;


		TopType = sTopType;
		BottomType = sBottomType;
		TieType = sTieType; 
		TopSkin = sTopSkin;
		BottomSkin = sBottomSkin;
		MannequinType = sMannequinType;

		mannequinMesh.GetComponent<MeshFilter>().mesh = mannequinMeshes[ MannequinType ].sharedMesh;
		mannequinMesh.GetComponent<MeshRenderer>().sharedMaterials = mannequinMeshRenders[ MannequinType ].sharedMaterials;


		if ( topMeshes.Length > 0 )
		{
			if ( sTopType >= 0 )
			{
				topMesh.SetActive( true );
				topMesh.GetComponent<MeshFilter>().mesh = topMeshes[ TopType ].sharedMesh;
				topMesh.GetComponent<MeshRenderer>().sharedMaterials = topMeshRenderers[ TopType ].sharedMaterials; 

				if ( sTopType != 8 )	// 8 is a special case as it has 2 materials, so we need to deal with it separately
					topMesh.GetComponent<MeshRenderer>().material = topSkins[ TopType ].Skins[ TopSkin ];
				else
				{
					Material[] tempTopSkin;
					tempTopSkin = topMesh.GetComponent<MeshRenderer>().sharedMaterials;
					tempTopSkin[0] = topSkins[ TopType ].Skins[ 0 ];
					tempTopSkin[1] = topSkins[ TopType ].Skins[ 1 ];

					topMesh.GetComponent<MeshRenderer>().sharedMaterials = tempTopSkin;
				}
			}
			else if ( sTopType < 0 )
			{
				topMesh.SetActive( false );
			}
		}

		if ( bottomMeshes.Length > 0 )
		{
			if ( sBottomType >= 0 )
			{
				bottomMesh.SetActive( true );
				bottomMesh.GetComponent<MeshFilter>().mesh = bottomMeshes[ BottomType ].sharedMesh;
				bottomMesh.GetComponent<MeshRenderer>().material = bottomSkins[ BottomType ].Skins[ BottomSkin ];
			}
			else if ( sBottomType < 0 )
				bottomMesh.SetActive( false );
		}

		if ( tieMeshes.Length > 0 )
		{
			if ( sTieType >= 0 )
			{
				tieMesh.SetActive( true );
				tieMesh.GetComponent<MeshFilter>().mesh = tieMeshes[ TieType ].sharedMesh;
			}
			else if ( sTieType < 0 )
				tieMesh.SetActive( false );
		}


		// Handle Mannequin skins

		if ( bUsesMannequinMasks )
		{
			Material[] newMannequinSkin;
			newMannequinSkin = mannequinMesh.GetComponent<MeshRenderer>().sharedMaterials;

			if ( MannequinType == 0 )
			{
				if ( sTopType < 0 )
					newMannequinSkin[1] = mannequinTopMaskOff;
				else if ( sTopType >= 0 )
					newMannequinSkin[1] = mannequinTopMaskMaterials[ TopType ];

				if ( newMannequinSkin.Length > 1 )
				{
					if ( sBottomType < 0 )
						newMannequinSkin[0] = mannequinBottomOff;
					else if ( sBottomType >= 0  &&   mannequinBottomMaskMaterials.Length > 0 )
						newMannequinSkin[0] = mannequinBottomMaskMaterials[ BottomType ];
				}
			}
			else
			{
				if ( sTopType < 0 )
					newMannequinSkin[0] = mannequinTopMaskOff;
				else if ( sTopType >= 0 )
					newMannequinSkin[0] = mannequinTopMaskMaterials[ TopType ];

				if ( newMannequinSkin.Length > 1 )
				{
					if ( sBottomType < 0 )
						newMannequinSkin[1] = mannequinBottomOff;
					else if ( sBottomType >= 0  &&   mannequinBottomMaskMaterials.Length > 0 )
						newMannequinSkin[1] = mannequinBottomMaskMaterials[ BottomType ];
				}
			}

			// Set alpha masking of the mannequin so polygons dont stick out of the clothings
			mannequinMesh.GetComponent<MeshRenderer>().sharedMaterials = newMannequinSkin;
		}

	}
}