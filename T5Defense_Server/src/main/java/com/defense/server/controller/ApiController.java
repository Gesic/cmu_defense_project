package com.defense.server.controller;

import com.defense.server.jwt.util.ResultCode;
import com.defense.server.jwt.util.ResultJson;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.bind.annotation.RestController;

import javax.servlet.http.HttpServletRequest;

@RestController
@RequestMapping("/api/v1")
public class ApiController {

	@GetMapping("/hello")
	@ResponseBody
	public ResultJson hello(HttpServletRequest request) {
		ResultJson resultJson = new ResultJson();
		resultJson.setCode(ResultCode.SUCCESS.getCode());
		resultJson.setMsg("Hello, " + request.getSession().getAttribute("userId").toString());
		return resultJson;
	}
}