package com.defense.server.entity;

import lombok.Getter;
import lombok.Setter;

import javax.persistence.*;
import java.time.LocalDateTime;

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

	@Column(name = "username", length = 50, nullable = true)
	private String username;

	@Column(name = "password", length = 100, nullable = true)
	private String password;

	@Column(name = "role", length = 20, nullable = true)
	private String role;

	@Column(name = "regdate")
	private LocalDateTime regDate;

}