package com.defense.server.login;

import java.io.File;
import java.io.FileReader;
import java.util.ArrayList;
import java.util.Base64;
import java.util.List;
import java.util.Scanner;

import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

import org.springframework.stereotype.Service;

import com.defense.server.entity.Plateinfo;
import com.defense.server.entity.Users;
import com.defense.server.repository.PlateRepository;
import com.defense.server.repository.UserRepository;

@Service
public class loginConfig {
	private final UserRepository userRepository;
    private final PlateRepository plateRepository;

    public loginConfig(UserRepository userRepository, PlateRepository plateRepository)
    {
    	this.userRepository = userRepository;
		this.plateRepository = plateRepository;
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

    				plateInfo.setLicensenumber(encrypt(dbList.get(0)));
    				plateInfo.setLicensestatus(encrypt(dbList.get(1)));
    				plateInfo.setLicenseexpdate(encrypt(dbList.get(2)));
    				plateInfo.setOwnername(encrypt(dbList.get(3)));
    				plateInfo.setOwnerbirthday(encrypt(dbList.get(4)));
    				plateInfo.setOwneraddress(encrypt(dbList.get(5)));
    				plateInfo.setOwnercity(encrypt(dbList.get(6)));
    				plateInfo.setVhemanufacture(encrypt(dbList.get(7)));
    				plateInfo.setVhemake(encrypt(dbList.get(8)));
    				plateInfo.setVhemodel(encrypt(dbList.get(9)));
    				plateInfo.setVhecolor(encrypt(dbList.get(10)));

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
    
    public static String alg = "AES/CBC/PKCS5Padding";
    private final String key = "01234567890123456789012345678901";
    private final String iv = key.substring(0, 16); // 16byte

    public String encrypt(String text) throws Exception {
        Cipher cipher = Cipher.getInstance(alg);
        SecretKeySpec keySpec = new SecretKeySpec(key.getBytes(), "AES");
        IvParameterSpec ivParamSpec = new IvParameterSpec(iv.getBytes());
        cipher.init(Cipher.ENCRYPT_MODE, keySpec, ivParamSpec);

        byte[] encrypted = cipher.doFinal(text.getBytes("UTF-8"));
        return Base64.getEncoder().encodeToString(encrypted);
    }

    public String decrypt(String cipherText) throws Exception {
        Cipher cipher = Cipher.getInstance(alg);
        SecretKeySpec keySpec = new SecretKeySpec(key.getBytes(), "AES");
        IvParameterSpec ivParamSpec = new IvParameterSpec(iv.getBytes());
        cipher.init(Cipher.DECRYPT_MODE, keySpec, ivParamSpec);

        byte[] decodedBytes = Base64.getDecoder().decode(cipherText);
        byte[] decrypted = cipher.doFinal(decodedBytes);
        return new String(decrypted, "UTF-8");
    }
}