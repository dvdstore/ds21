package com.ds2model;

public class PrevPurchase
{
	private String prev_title;
	private String prev_actor;
	private String recommend_title;
	
	public String getPrev_title()
	{
		return prev_title;
	}
	
	public void setPrev_title(String prm_prev_title)
	{
		this.prev_title = prm_prev_title;
	}
	
	public String getPrev_actor()
	{
		return prev_actor;
	}
	
	public void setPrev_actor(String prm_prev_actor)
	{
		this.prev_actor = prm_prev_actor;
	}
	
	public String getRecommend_title()
	{
		return recommend_title;
	}
	
	public void setRecommend_title(String prm_recommend_title)
	{
		this.recommend_title = prm_recommend_title;
	}
	
}