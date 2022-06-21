package com.defense.server.controller;

import java.util.List;
import java.util.Map;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.ResponseBody;

import com.defense.server.Plate.PlateNumberService;
import com.defense.server.entity.Plateinfo;

import lombok.RequiredArgsConstructor;

@RequiredArgsConstructor
@Controller
public class MainController {

	private final PlateNumberService plateNumberService;

	@RequestMapping(value = "/db", method = { RequestMethod.GET, RequestMethod.POST })
	public String home(Model model) {
		List<Plateinfo> plateNumberList = this.plateNumberService.getList();
		model.addAttribute("plateNumberList", plateNumberList);
		return "db_go"; // html file
	}

	@RequestMapping(value = "/", method = { RequestMethod.GET, RequestMethod.POST })
	public String root() {
		return "redirect:/db";
	}

	@ResponseBody
	@RequestMapping(value = "/testJSON", method = RequestMethod.POST)
	public List<Plateinfo> getResultJSON(@RequestBody Map<String, Object> recvInfo, Model model) {
		List<Plateinfo> result = this.plateNumberService.getQueryForPlateNumJSON(recvInfo.get("plateNum").toString());
		return result;
	}

//	@RequestMapping(value = "/test", method = RequestMethod.POST)
//	public String getResult(@RequestBody Map<String, Object> recvInfo, Model model) {
//		System.out.println("A : "+ recvInfo.get("id"));
//		System.out.println("B : "+ recvInfo.get("plateNum").toString());
//		String result = this.plateNumberService.getQueryForPlateNum(recvInfo.get("plateNum").toString());
//		model.addAttribute("RESULT", result);
//		return "result";
//	}

}
