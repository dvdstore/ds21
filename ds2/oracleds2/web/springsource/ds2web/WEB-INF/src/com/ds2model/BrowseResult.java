package com.ds2model;

public class BrowseResult
{
	private String prod_id;
	private String title;
	private String actor;
	private String price;
	
	public String getProd_id()
	{
		return prod_id;
	}
	
	public void setProd_id(String prm_prod_id)
	{
		this.prod_id = prm_prod_id;
	}
	
	public String getTitle()
	{
		return title;
	}
	
	public void setTitle(String prm_title)
	{
		this.title = prm_title;
	} 
	
	public String getActor()
	{
		return actor;
	}
	
	public void setActor(String prm_actor)
	{
		this.actor = prm_actor;
	} 
	
	public String getPrice()
	{
		return price;
	}
	
	public void setPrice(String prm_price)
	{
		this.price = prm_price;
	} 
	
}