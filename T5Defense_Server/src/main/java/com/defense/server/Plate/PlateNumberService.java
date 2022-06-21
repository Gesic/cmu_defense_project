package com.defense.server.Plate;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.defense.server.entity.Plateinfo;
import com.defense.server.repository.PlateRepository;

import lombok.RequiredArgsConstructor;

@RequiredArgsConstructor
@Service
public class PlateNumberService {

	@Autowired
	private final PlateRepository plateRepository;

	public List<Plateinfo> getList() {
		return this.plateRepository.findAll();
	}

	public List<Plateinfo> getQueryForPlateNumJSON(String platenum) {
		List<Plateinfo> searchresult = this.plateRepository.findByLicensenumber(platenum);
		return searchresult;
	}

//	public String getQueryForPlateNum(String platenum) {
//		System.out.println(platenum);
//		String result = "";
//		List<Plateinfo> searchresult = this.plateRepository.findByLicensenumber(platenum);
//		result += searchresult.get(0).getLicensenumber() + "\n";
//		result += searchresult.get(0).getLicensestatus() + "\n";
//		result += searchresult.get(0).getOwnername() + "\n";
//		result += searchresult.get(0).getOwnerbirthday() + "\n";
//		result += searchresult.get(0).getOwneraddress() + "\n";
//		result += searchresult.get(0).getOwnercity() + "\n";
//		result += searchresult.get(0).getVhemanufacture() + "\n";
//		result += searchresult.get(0).getVhemake() + "\n";
//		result += searchresult.get(0).getVhemodel() + "\n";
//		result += searchresult.get(0).getVhecolor() + "\n";
//		System.out.println(result);
//		return result;
//	}

}