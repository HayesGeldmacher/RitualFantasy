//JPGPU:
//a JPEG compression post post processing effect
//by ompu co | Sam Blye (c) 2021, all rights reserved


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]

public class JPGPU : MonoBehaviour {
	public enum bS{ONE=1,TWO=2,FOUR=4,EIGHT=8,SIXTEEN=16,THIRTYTWO=32,SIXTYFOUR=64};

	public bS blockSize = bS.EIGHT;
	[Range(1.0F,16.0F)]
	public float quality = 16.0f;

	public bool downscale = false;

	[Range(.01f,10f)]
	public float truncationLoss = 0.01f;

	[Range(-2f,2f)]
	public float DCTContrast = 0;



	
	[Range(0.0f,32.0f)]
	public float sharpen = 0.0f;






	[SerializeField, HideInInspector]
	private Shader m_Shader;

	public Shader shader
	{
		get
		{
			if (m_Shader == null)
			{
				const string shaderName = "Hidden/JPGPU";
				m_Shader = Shader.Find(shaderName);
			}

			return m_Shader;
		}
	}

	private Material m_Material;
	public Material material
	{
		get
		{
			if (m_Material == null)
				m_Material = new Material(shader);

			return m_Material;
		}
	}

	[SerializeField, HideInInspector]
	private Shader m_sharpShader;

	public Shader sshader
	{
		get
		{
			if (m_sharpShader == null)
			{
				const string sharpShaderName = "Hidden/Sharpen";
				m_sharpShader = Shader.Find(sharpShaderName);
			}

			return m_sharpShader;
		}
	}

	private Material m_sharpMat;
	public Material sharpMat
	{
		get
		{
			if (m_sharpMat == null)
				m_sharpMat = new Material(sshader);

			return m_sharpMat;
		}
	}

	private void OnDisable()
	{
		if (m_Material != null)
			DestroyImmediate(m_Material);

		m_Material = null;
	}




	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		RenderTexture buf1 = RenderTexture.GetTemporary(downscale?Mathf.FloorToInt(src.width/4)*2:Mathf.FloorToInt(src.width/2)*2, downscale?Mathf.FloorToInt(src.height/4)*2:Mathf.FloorToInt(src.height/2)*2, src.depth, RenderTextureFormat.ARGBHalf);
		//RenderTexture buf1 = RenderTexture.GetTemporary(src.width,src.height, src.depth, src.format);
		RenderTexture buf2 = RenderTexture.GetTemporary(buf1.width,buf1.height, src.depth, RenderTextureFormat.ARGBHalf);
		//buf1.filterMode=FilterMode.Point;
		//buf2.filterMode=FilterMode.Point;
		//buf1.antiAliasing=src.antiAliasing;
		//buf2.antiAliasing=src.antiAliasing;
//		Debug.Log(buf1.width +" and " +buf1.height);
	
		material.SetInt("_BS",(int)blockSize);
		material.SetInt("_DS",downscale?1:0);
		material.SetFloat("_Quality",quality);
		
		//mat.SetInt("_Truncate",truncatedCompression?1:0);
		material.SetFloat("_Truncation",truncationLoss);

		material.SetFloat("_Contrast",DCTContrast);

	
		if(sharpen > 0.00f){
			RenderTexture buf0 = RenderTexture.GetTemporary(src.width,src.height, src.depth, RenderTextureFormat.ARGBHalf);
			//buf0.antiAliasing=src.antiAliasing;


			sharpMat.SetFloat("_Sharpness",sharpen);
			sharpMat.SetFloat("_BufferDownscale",downscale?2:1);
			Graphics.Blit(src,buf0,sharpMat);
			Graphics.Blit(buf0,buf1,material,0);
			Graphics.Blit(buf1,buf2,material,1);
			Graphics.Blit(buf2,dest,material,2);


			/*
			Graphics.Blit(src,buf0,sharpMat);
			Graphics.Blit(buf0,buf1,mat,0);
			Graphics.Blit(buf1,buf2,mat,1);
			Graphics.Blit(buf2,dest,mat,2);
			*/

			RenderTexture.ReleaseTemporary(buf0);
		}
		else{	
			Graphics.Blit(src,buf1,material,0);
			Graphics.Blit(buf1,buf2,material,1);
			Graphics.Blit(buf2,dest,material,2);
		}
	
	

		RenderTexture.ReleaseTemporary(buf1);
		RenderTexture.ReleaseTemporary(buf2);
	
		
	}
}
