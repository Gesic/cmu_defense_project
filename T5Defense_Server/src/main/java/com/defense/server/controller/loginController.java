package com.defense.server.controller;

import java.time.Duration;
import java.time.LocalDateTime;
import java.util.Map;
import java.util.concurrent.ThreadLocalRandom;

import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.bind.annotation.RestController;

import com.defense.server.entity.Users;
import com.defense.server.login.loginService;
import com.defense.server.repository.UserRepository;

import lombok.RequiredArgsConstructor;

@RestController
@RequestMapping("/auth")
@RequiredArgsConstructor
public class loginController {
	private final UserRepository userRepository;

    /*
     * JSON format
     * {
     *     "userid" : "ID"
     *     "password" : "123456"
     * }
     * */
	@ResponseBody
	@RequestMapping("/login")
	public String login(@RequestBody Map<String, Object> recvInfo, Model model) {

		String userid = recvInfo.get("userid").toString();
		String password = recvInfo.get("password").toString();

		Users userInfo = this.userRepository.findUserByUserid(userid);
		
		if(password.equals(userInfo.getPassword()))
		{
			this.sendMail(userInfo);
		}
		else
		{
			return "Wrong Password";
		}
		
		return "login Success\n"+"Please check OTP";
	}
	
	// 2nd Authentication - email-authentication(Link)
    private final loginService loginService;

    public String sendMail(Users user) {
    	String result;
    	
    	// create OTP key (6 digit)
    	int otpKey = ThreadLocalRandom.current().nextInt(100000, 1000000);
    	user.setOtpKey(otpKey);
    	
    	// Current time for OTP Timeout
    	LocalDateTime now = LocalDateTime.now();
    	user.setRegDate(now);
    	
    	this.userRepository.save(user);
    	
        result = loginService.sendOtpMail(user.getEmail(), otpKey);
        return result;
    }

    
    /*
     * JSON format
     * {
     *     "userid" : "ID"
     *     "otpKey" : "123456"
     * }
     * */
    @ResponseBody
    @RequestMapping("/otp-check")
    public String signUpConfirm(@RequestBody Map<String, Object> recvInfo, Model model) {
    	
    	String userid = recvInfo.get("userid").toString();
		String otpKey = recvInfo.get("otpKey").toString();
    	
    	Users userInfo = this.userRepository.findUserByUserid(userid);
    	LocalDateTime now = LocalDateTime.now();
    	Duration duration = Duration.between(userInfo.getRegDate(), now);

    	int durationOfTimeout = 600;

    	if (userInfo.getOtpKey() == Integer.parseInt(otpKey) && duration.getSeconds() < durationOfTimeout)
    	{
    		userInfo.setCheck2ndauth("SUCCESS");
    		this.userRepository.save(userInfo);
    	}
    	else
    	{
    		return "Fail to 2FA";
    	}
    	
    	return "Success to 2FA";
   }
}