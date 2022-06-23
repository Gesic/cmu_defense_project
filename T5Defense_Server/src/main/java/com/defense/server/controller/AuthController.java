//package com.defense.server.controller;
//
//import com.defense.server.entity.Users;
//import com.defense.server.jwt.util.JwtUtil;
//import com.defense.server.jwt.util.ResultCode;
//import com.defense.server.jwt.util.ResultJson;
//import com.defense.server.repository.UserRepository;
//
//import lombok.RequiredArgsConstructor;
//import org.springframework.security.crypto.factory.PasswordEncoderFactories;
//import org.springframework.security.crypto.password.PasswordEncoder;
//import org.springframework.web.bind.annotation.*;
//
//@RestController
//@RequestMapping("/auth/v1")
//@RequiredArgsConstructor
//public class AuthController {
//	private final UserRepository userRepository;
//	private final JwtUtil jwtUtil;
//
//	@PostMapping("/login")
//	@ResponseBody
//	public ResultJson login(@RequestParam(name = "userid", required = true) String userid,
//			@RequestParam(name = "password", required = true) String password) {
//
//		ResultJson resultJson = new ResultJson();
//		Users users = userRepository.findUserByUserid(userid);
//		String token = "";
//		PasswordEncoder passwordEncoder = PasswordEncoderFactories.createDelegatingPasswordEncoder();
//		if (users != null && passwordEncoder.matches(password, users.getPassword())) {
//			token = jwtUtil.generateToken(users);
//		} else {
//			resultJson.setCode(ResultCode.LOGIN_FAIL.getCode());
//			resultJson.setMsg(ResultCode.LOGIN_FAIL.getMsg());
//			return resultJson;
//		}
//		resultJson.setCode(ResultCode.SUCCESS.getCode());
//		resultJson.setMsg(ResultCode.SUCCESS.getMsg());
//		resultJson.setToken(token);
//		return resultJson;
//	}
//}