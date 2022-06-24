package com.defense.server.login;

import lombok.Getter;
import lombok.RequiredArgsConstructor;
import lombok.Setter;

@Getter
@Setter
@RequiredArgsConstructor
public class loginMessage {
	private String userid;
    private String otpKey;
}