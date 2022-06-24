package com.defense.server.login;

import java.util.List;

import javax.mail.MessagingException;
import javax.mail.internet.MimeMessage;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.mail.javamail.JavaMailSender;
import org.springframework.mail.javamail.MimeMessageHelper;
import org.springframework.stereotype.Service;

import com.defense.server.entity.Plateinfo;
import com.defense.server.entity.Users;
import com.defense.server.repository.PlateRepository;
import com.defense.server.repository.UserRepository;

@Service
public class loginService {
    private JavaMailSender javaMailSender;
    private final UserRepository userRepository;

    public loginService(JavaMailSender javaMailSender, UserRepository userRepository)
    {
    	this.userRepository = userRepository;
		this.javaMailSender = javaMailSender;
    }

	public List<Users> getList() {
		return this.userRepository.findAll();
	}

	public Users getQueryForUserIdJSON(String userid) {
		Users searchresult = this.userRepository.findUserByUserid(userid);
		return searchresult;
	}
    public String sendOtpMail(String userEmail, int otpKey) {
        MimeMessage mimeMessage = javaMailSender.createMimeMessage();
        try {
            MimeMessageHelper mimeMessageHelper = new MimeMessageHelper(mimeMessage, true, "UTF-8");
            mimeMessageHelper.setTo(userEmail); // 메일 수신자
            mimeMessageHelper.setSubject("[System] Check second authentication"); // 메일 제목
            mimeMessageHelper.setText("OTP:"+otpKey, false); // 메일 본문 내용, HTML 여부
            javaMailSender.send(mimeMessage);
            return "Email Send Success!!";
        } catch (MessagingException e) {
            throw new RuntimeException(e);
        }
    }
}
