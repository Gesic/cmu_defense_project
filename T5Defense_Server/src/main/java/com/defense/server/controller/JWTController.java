package com.defense.server.controller;

import javax.servlet.http.HttpServletRequest;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.ResponseBody;

import com.defense.server.jwt.util.ResultCode;
import com.defense.server.jwt.util.ResultJson;

import lombok.RequiredArgsConstructor;

@RequiredArgsConstructor
@Controller
public class JWTController {

	@ResponseBody
	@RequestMapping(value = "/hello", method = { RequestMethod.GET, RequestMethod.POST })
	public ResultJson hello(HttpServletRequest request) {
		ResultJson resultJson = new ResultJson();
		resultJson.setCode(ResultCode.SUCCESS.getCode());
		resultJson.setMsg("Hello, " + request.getSession().getAttribute("userId").toString());
		return resultJson;
	}

}
