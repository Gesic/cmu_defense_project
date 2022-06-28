package com.defense.server.controller;

import java.time.Duration;
import java.time.LocalDateTime;
import java.util.Map;
import java.util.concurrent.ThreadLocalRandom;

import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.bind.annotation.RestController;

import com.defense.server.entity.Users;
import com.defense.server.jwt.util.JwtUtil;
import com.defense.server.jwt.util.ResultCode;
import com.defense.server.jwt.util.ResultJson;
import com.defense.server.login.loginService;
import com.defense.server.repository.UserRepository;

import lombok.RequiredArgsConstructor;

@RestController
@RequestMapping("/auth")
@RequiredArgsConstructor
public class loginController {
	private final int PW_FAIL_COUNT = 5;
	private final long OTP_TIMEOUT_SEC = 60 * 10;
	private final String OTP_STATUS_START = "start";
	private final String OTP_STATUS_FINISHED = "finished";
	
	private final UserRepository userRepository;
	private final JwtUtil jwtUtil;

	// 2nd Authentication - email-authentication(Link)
    private final loginService loginService;
    /*
     * JSON format
     * {
     *     "userid":"ID",
     *     "password":"123456"
     * }
     * */
	@ResponseBody
	@PostMapping("/login")
	public ResultJson login(@RequestBody Map<String, Object> recvInfo) {
		ResultJson resultJson = new ResultJson();
		Users userInfo;
		
		try {
			if (recvInfo == null) {
				throw new Exception("Invalid arguments");
			}
			
			Object userid = recvInfo.get("userid");
			Object password = recvInfo.get("password");
			if (userid == null || password == null) {
				throw new Exception("Invalid arguments");
			}
			
			// Create User/Car DB, if DB is empty.
			if (userRepository.count() == 0)
			{
				loginService.createUserDB();
				loginService.createPlateInfoDB();
			}

			userInfo = userRepository.findUserByUserid(userid.toString());
			if (userInfo == null) {
				throw new Exception("The user does not exist");
			}

			if(userInfo.getFailcount() == PW_FAIL_COUNT)
			{
				throw new Exception("Please contact administrator");
			}

			if (!password.toString().equals(userInfo.getPassword())) {
				userInfo.setFailcount(userInfo.getFailcount() + 1);
				userRepository.save(userInfo);
				throw new Exception("Invalid password, Fail Count:" + userInfo.getFailcount());
			}

			sendMail(userInfo);

			resultJson.setCode(ResultCode.SUCCESS.getCode());
			resultJson.setMsg(ResultCode.SUCCESS.getMsg());
		} catch (Exception e) {
			resultJson.setCode(ResultCode.LOGIN_FAIL.getCode());
			resultJson.setMsg(ResultCode.LOGIN_FAIL.getMsg() + " Reason: " + e.getMessage());
		}
		
		return resultJson;
	}
	
    public String sendMail(Users user) {
    	String result;
    	
		// Reset fail counts of password
		user.setFailcount(0);

    	// create OTP key (6 digit)
    	int otpKey = ThreadLocalRandom.current().nextInt(100000, 1000000);
    	user.setOtpKey(otpKey);

    	// Current time for OTP Timeout
    	LocalDateTime now = LocalDateTime.now();
    	user.setRegDate(now);
    	user.setCheck2ndauth(OTP_STATUS_START);
    	userRepository.save(user);
    	
        result = loginService.sendOtpMail(user.getEmail(), otpKey);
        return result;
    }

    /*
     * JSON format
     * {
     *     "userid":"ID",
     *     "otpKey":"123456"
     * }
     * */
    @ResponseBody
    @PostMapping("/otp-check")
    public ResultJson signUpConfirm(@RequestBody Map<String, Object> recvInfo) {
    	ResultJson resultJson = new ResultJson();
    	Users userInfo;
    	try {
			if (recvInfo == null) {
				throw new Exception("Invalid arguments");
			}
			
			Object userid = recvInfo.get("userid");
			Object otpKey = recvInfo.get("otpKey");
			if (userid == null || otpKey == null) {
				throw new Exception("Invalid arguments");
			}
			
			userInfo = userRepository.findUserByUserid(userid.toString());
			if (userInfo == null) {
				throw new Exception("The user does not exist");
			}
			
			String otpStatus = userInfo.getCheck2ndauth();
			if (!otpStatus.equals(OTP_STATUS_START)) {
				throw new Exception("OTP session terminated");
			}
			
	    	Duration duration = Duration.between(userInfo.getRegDate(), LocalDateTime.now());
	    	if (duration.getSeconds() > OTP_TIMEOUT_SEC) {
	    		userInfo.setCheck2ndauth(OTP_STATUS_FINISHED);
	    		userRepository.save(userInfo);
	    		throw new Exception("OTP session terminated");
	    	}
	    	
	    	if (userInfo.getOtpKey() != Integer.parseInt(otpKey.toString())) {
	    		throw new Exception("Invalid OTP");
	    	}
	    	
	    	userInfo.setCheck2ndauth(OTP_STATUS_FINISHED);
    		userRepository.save(userInfo);
    		
    		String token = jwtUtil.generateToken(userInfo);
    		
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