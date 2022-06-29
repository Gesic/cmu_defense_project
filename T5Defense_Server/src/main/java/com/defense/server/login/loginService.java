package com.defense.server.login;

import java.io.File;
import java.io.FileReader;
import java.util.ArrayList;
import java.util.List;
import java.util.Scanner;

import javax.mail.MessagingException;
import javax.mail.internet.MimeMessage;

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
    private final PlateRepository plateRepository;

    public loginService(JavaMailSender javaMailSender, UserRepository userRepository, PlateRepository plateRepository)
    {
    	this.userRepository = userRepository;
		this.javaMailSender = javaMailSender;
		this.plateRepository = plateRepository;
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
    
    // Create User Credential DB with "userData.txt"
    public String createUserDB()
    {
    	try {
    		Users tempUser;
    		
    		File file = new File("./userData.txt");
    		Scanner fileScanner = new Scanner(file);
    		String sysOut = null;
    		while(fileScanner.hasNext())
    		{
    			tempUser = new Users();
    			sysOut = fileScanner.nextLine();
    			tempUser.setUserid(sysOut);
    			sysOut = fileScanner.nextLine();
    			tempUser.setPassword(sysOut);
    			sysOut = fileScanner.nextLine();
    			tempUser.setEmail(sysOut);
    			userRepository.save(tempUser);
    		}
    		fileScanner.close();
    		return "Success";
    	} catch (Exception e) {
    		System.out.println(e.getMessage());
    		return "Fail"; 
    	}
    }

    // Create Plate Info DB with "datafile.txt"
    public String createPlateInfoDB()
    {
    	try {
    		Plateinfo plateInfo = null;

    		FileReader fileReader = new FileReader("./datafile.txt");
    		int singleCh = 0;
    		String tempDB = "";
    		List<String> dbList = new ArrayList<String>();

    		while((singleCh = fileReader.read()) != -1)
    		{
    			if (singleCh == 13) // Check CR
    			{
    				dbList.add(tempDB);
    				tempDB = "";
    			}
    			else if (singleCh == 10) // Skip LF
    			{
    				// Do nothing
    			}
    			else if((char)singleCh == '$')
    			{
    				dbList.add(tempDB);
    				plateInfo = new Plateinfo();

    				plateInfo.setLicensenumber(dbList.get(0));
    				plateInfo.setLicensestatus(dbList.get(1));
    				plateInfo.setLicenseexpdate(dbList.get(2));
    				plateInfo.setOwnername(dbList.get(3));
    				plateInfo.setOwnerbirthday(dbList.get(4));
    				plateInfo.setOwneraddress(dbList.get(5));
    				plateInfo.setOwnercity(dbList.get(6));
    				plateInfo.setVhemanufacture(dbList.get(7));
    				plateInfo.setVhemake(dbList.get(8));
    				plateInfo.setVhemodel(dbList.get(9));
    				plateInfo.setVhecolor(dbList.get(10));

    				plateRepository.save(plateInfo);

    				dbList.clear();
    				tempDB = "";
    			}
    			else
    			{
    				tempDB += (char)singleCh;
    			}
    		}
    		fileReader.close();
    		return "Success";
    	} catch (Exception e) {
    		System.out.println(e.getMessage());
    		return "Fail"; 
    	}
    }
}
