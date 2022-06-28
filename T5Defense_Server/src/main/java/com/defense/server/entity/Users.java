package com.defense.server.entity;

import java.time.LocalDateTime;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.Table;

import lombok.Getter;
import lombok.Setter;

@Entity
@Getter
@Setter
@Table(name = "users")
public class Users {
	@Id
	@GeneratedValue(strategy = GenerationType.IDENTITY)
	@Column(name = "userkey", length = 50, nullable = true)
	private Long userkey;

	@Column(name = "userid", length = 50, nullable = true, unique = true)
	private String userid;

	@Column(name = "password", length = 100, nullable = true)
	private String password;

	@Column(name = "regdate")
	private LocalDateTime regDate;

	@Column(name = "email", length = 50, nullable = true)
	private String email;

	@Column(name = "otpKey", length = 10, nullable = true)
	public int otpKey;

	@Column(name = "check2ndauth", length = 20, nullable = true)
	private String check2ndauth;
}