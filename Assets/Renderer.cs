using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Renderer : MonoBehaviour
{
	public Texture2D renderer_tex;
	public Camera cam;
	public float dis;

	public float r;
	public Vector3 center;

	public Vector3 boxSize;

	public Vector3 light;
	public float spec;
    // Start is called before the first frame update
    void Start()
    {
    	

    	Vector3[] corners=new Vector3[4];

    	float halfFov=0.5f*cam.fieldOfView*Mathf.Deg2Rad;
    	float aspect=cam.aspect;

    	float height=dis*Mathf.Tan(halfFov);
    	float width=height*aspect;

		Transform tx=cam.transform;

    	corners[0]=tx.position-width*tx.right+height*tx.up+dis*tx.forward;
    	corners[1]=tx.position+width*tx.right+height*tx.up+dis*tx.forward;
    	corners[2]=tx.position-width*tx.right-height*tx.up+dis*tx.forward;
    	corners[3]=tx.position+width*tx.right-height*tx.up+dis*tx.forward;
        
        Debug.Log(corners[0]);
        Debug.Log(corners[1]);
        Debug.Log(corners[2]);
        Debug.Log(corners[3]);

        Vector3 begin=corners[0];
        Vector3 end=corners[3];

		Debug.Log((int)(end.x-begin.x));
		Debug.Log((int)(end.y-begin.y));

		int w=(int)(end.x-begin.x);
		int h=Mathf.Abs((int)(end.y-begin.y));

        renderer_tex=new Texture2D(w,h,TextureFormat.ARGB32, false);

        Vector3[,] dirs=new Vector3[w,h];

        Vector3 normal=Vector3.Cross(corners[0]-corners[1],corners[3]-corners[1]);

        for(int i=0;i<w;i++)
        {
        	for(int j=0;j<h;j++)
        	{	Color c=new Color();
        		c.a=0.0f;
        		renderer_tex.SetPixel(i,j,c);
        	}
        }

        for(int i=0;i<w;i+=5)
        {
        	for(int j=0;j<h;j+=5)
        	{
        		float z=corners[0].z-(normal.x*(i-corners[0].x)+normal.y*(j-corners[0].y))/normal.z;//法线式求点和平面的关系
        		Vector3 pixel_pos=new Vector3(i+(int)(begin.x),(int)(begin.y)-j,z);//从上往下遍历，y递减
        		dirs[i,j]=Vector3.Normalize(pixel_pos-tx.position);
        		
        		Color c=new Color();
        		c.a=0.0f;

        		Vector3 pos=tx.position;
        		for(int m=0;m<50;m++)
        		{
	        		//float dis=union(SDFBox(pos),SDFBall(pos));
	        		//float dis=intersect(SDFBox(pos),SDFBall(pos));
	        		float dis=subtract(SDFBox(pos),SDFBall(pos));
	        		//float dis=subtract(SDFBall(pos),SDFBox(pos));
	        		//float dis=SDFBall(pos);

	        		if(dis<0.1)
	        		{
	        			Vector3 E=-dirs[i,j];
	        			Vector3 N=Vector3.Normalize(pos-center);
	        			Vector3 L=Vector3.Normalize(light);
	        			Vector3 H=(L+E)/(L+E).magnitude;
	        			float diffuse=Vector3.Dot(N,L);
	        			float specular=Mathf.Pow(Vector3.Dot(N,H),spec);
	        			float lux=diffuse+specular;

	        			c.r=0.6f;
	        			c.b=0.1f;
	        			c.g=0.3f;

	        			c.r+=lux;
	        			c.g+=lux;
	        			c.b+=lux;
	        			c.a=1.0f;
	        			break;
	        		}
	        		else
	        		{
	        			pos+=dirs[i,j]*dis;
	        		}
				}

        		renderer_tex.SetPixel(i,h-j-1,c);//Y坐标反的
        	}
        }

      	byte[] pngShot = renderer_tex.EncodeToPNG();
        File.WriteAllBytes("export.png", pngShot);


    }

    float SDFBall(Vector3 pos)
   	{
   		return Vector3.Distance(pos,center)-r;
   	}	

	float SDFBox(Vector3 pos) 
	{

  		Vector3 d=pos - center;
  		d.x=Mathf.Abs(d.x);
  		d.y=Mathf.Abs(d.y);
  		d.z=Mathf.Abs(d.z);
  		d-=boxSize;

  		Vector3 dd;
  		dd.x=Mathf.Max(d.x,0);
  		dd.y=Mathf.Max(d.y,0);
  		dd.z=Mathf.Max(d.z,0);

  		return dd.magnitude+Mathf.Min(Mathf.Max(Mathf.Max(d.x,d.y),d.z),0.0f);

    }


    float union(float a,float b)
    {
    	return a<b?a:b;
    }

    float intersect(float a,float b)
    {	
    	return a>b?a:b;
    }

    float subtract(float a,float b)
    {
    	return a>-b?a:-b;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
