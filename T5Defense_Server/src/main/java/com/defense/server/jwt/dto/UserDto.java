package com.defense.server.jwt.dto;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class UserDto {
	private String userid;
	private String password;
	private String userName;
	private String role;
	private String regDate;
}