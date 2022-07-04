package com.defense.server.controller;

import java.time.Duration;
import java.time.LocalDateTime;
import java.util.Map;
import java.util.concurrent.ThreadLocalRandom;

import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.bind.annotation.RestController;

import com.defense.server.entity.Users;
import com.defense.server.jwt.util.JwtUtil;
import com.defense.server.jwt.util.ResultCode;
import com.defense.server.jwt.util.ResultJson;
import com.defense.server.login.LoginConfig;
import com.defense.server.login.LoginService;
import com.defense.server.repository.UserRepository;

import lombok.RequiredArgsConstructor;

@RestController
@RequestMapping("/auth")
@RequiredArgsConstructor
public class LoginController {
	private final int PW_FAIL_COUNT = 5;
	private final long OTP_TIMEOUT_SEC = 60 * 10;
	private final String OTP_STATUS_START = "start";
	private final String OTP_STATUS_FINISHED = "finished";
	
	private final UserRepository userRepository;
	private final JwtUtil jwtUtil;

    private final LoginService loginService;
    private final LoginConfig loginConfig;
    private final BCryptPasswordEncoder bcryptEncoder = new BCryptPasswordEncoder();
    
    /*
     * !!!  C A U T I O N  !!!
     * This function is only added to help set the environment for Team 1.
     * In the real environment, this code does not exist,
     * and it is assumed that users are pre-registered in the DB.
     * Therefore, do not assume this code to be a vulnerability.
     * */
    /*
    @ResponseBody
    @PostMapping("/signup")
    public ResultJson signUp(@RequestBody Map<String, Object> recvInfo) {
    	ResultJson resultJson = new ResultJson();
    	
    	try {
    		if (recvInfo == null) {
				throw new Exception("Invalid arguments");
			}
    		
    		Object userid = recvInfo.get("userid");
			Object password = recvInfo.get("password");
			Object email = recvInfo.get("email");
			if (userid == null || password == null || email == null) {
				throw new Exception("Invalid arguments");
			}
			
			Users user = userRepository.findUserByUserid(userid.toString());
			if (user != null) {
				throw new Exception("The user already exist");
			}
			
			user = new Users();
			user.setUserid(userid.toString());
			user.setPassword(bcryptEncoder.encode(password.toString()));
			user.setEmail(loginConfig.encrypt(email.toString()));
			userRepository.save(user);
			
			resultJson.setCode(ResultCode.SUCCESS.getCode());
			resultJson.setMsg(ResultCode.SUCCESS.getMsg());
			resultJson.setToken("");
    	} catch (Exception e) {
    		resultJson.setCode(ResultCode.SERVER_ERROR.getCode());
			resultJson.setMsg(ResultCode.SERVER_ERROR.getMsg() + " Reason: " + e.getMessage());
			resultJson.setToken("");
    	}
    	
    	return resultJson;
    }
    */
    
	@ResponseBody
	@PostMapping("/login")
	public ResultJson login(@RequestBody Map<String, Object> recvInfo) {
		ResultJson resultJson = new ResultJson();
		
		try {
			if (recvInfo == null) {
				throw new Exception("Invalid arguments");
			}
			
			Object userid = recvInfo.get("userid");
			Object password = recvInfo.get("password");
			if (userid == null || password == null) {
				throw new Exception("Invalid arguments");
			}
			
			Users user = userRepository.findUserByUserid(userid.toString());
			if (user == null) {
				throw new Exception("The user does not exist");
			}

			if (user.getFailcount() >= PW_FAIL_COUNT) {
				throw new Exception("Please contact administrator");
			}

			if (!bcryptEncoder.matches(password.toString(), user.getPassword())) {
				user.setFailcount(user.getFailcount() + 1);
				userRepository.save(user);
				throw new Exception("Invalid password, Fail Count:" + user.getFailcount());
			}
			
			user.setFailcount(0);
			sendMail(user);

			resultJson.setCode(ResultCode.SUCCESS.getCode());
			resultJson.setMsg(ResultCode.SUCCESS.getMsg());
			resultJson.setToken("");
		} catch (Exception e) {
			resultJson.setCode(ResultCode.LOGIN_FAIL.getCode());
			resultJson.setMsg(ResultCode.LOGIN_FAIL.getMsg() + " Reason: " + e.getMessage());
			resultJson.setToken("");
		}
		
		return resultJson;
	}
	
    private String sendMail(Users user) throws Exception {
    	if (user == null) {
    		throw new Exception("Invalid arguments");
    	}
    	
    	final String email = loginConfig.decrypt(user.getEmail());
    	final int otpKey = ThreadLocalRandom.current().nextInt(100000, 1000000);
    	final String encodedOtpKey = bcryptEncoder.encode(Integer.toString(otpKey));
    	
    	user.setOtpKey(encodedOtpKey);
    	user.setRegDate(LocalDateTime.now());
    	user.setCheck2ndauth(OTP_STATUS_START);
    	userRepository.save(user);
    	
        return loginService.sendOtpMail(email, otpKey);
    }
    
    @ResponseBody
    @PostMapping("/otp-check")
    public ResultJson signUpConfirm(@RequestBody Map<String, Object> recvInfo) {
    	ResultJson resultJson = new ResultJson();
    	Users user;
    	try {
			if (recvInfo == null) {
				throw new Exception("Invalid arguments");
			}
			
			Object userid = recvInfo.get("userid");
			Object otpKey = recvInfo.get("otpKey");
			if (userid == null || otpKey == null) {
				throw new Exception("Invalid arguments");
			}
			
			user = userRepository.findUserByUserid(userid.toString());
			if (user == null) {
				throw new Exception("The user does not exist");
			}
			
			String otpStatus = user.getCheck2ndauth();
			if (!otpStatus.equals(OTP_STATUS_START)) {
				throw new Exception("OTP session terminated");
			}
			
	    	Duration duration = Duration.between(user.getRegDate(), LocalDateTime.now());
	    	if (duration.getSeconds() > OTP_TIMEOUT_SEC) {
	    		user.setCheck2ndauth(OTP_STATUS_FINISHED);
	    		userRepository.save(user);
	    		throw new Exception("OTP session terminated");
	    	}
	    	
	    	if (!bcryptEncoder.matches(otpKey.toString(), user.getOtpKey())) {
	    		throw new Exception("Invalid OTP");
	    	}
	    	
	    	user.setCheck2ndauth(OTP_STATUS_FINISHED);
    		userRepository.save(user);
    		
    		String token = jwtUtil.generateToken(user);
    		
    		resultJson.setCode(ResultCode.SUCCESS.getCode());
    		resultJson.setMsg(ResultCode.SUCCESS.getMsg());
    		resultJson.setToken(token);
		} catch (Exception e) {
			resultJson.setCode(ResultCode.LOGIN_FAIL.getCode());
			resultJson.setMsg(ResultCode.LOGIN_FAIL.getMsg() + " Reason: " + e.getMessage());
			resultJson.setToken("");
		}

    	return resultJson;
   }
}