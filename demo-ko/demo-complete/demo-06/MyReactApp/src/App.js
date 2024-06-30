import { useEffect, useRef, useState } from "react";
import "./App.css";

function App({ weatherApi }) {

  const [allZones, setAllZones] = useState([]);
  const [zones, setZones] = useState([]);

  const [ filter, setFilter ] = useState({
    sido: "",
    sigungu: ""
  });

  const [forecasts, setForecasts] = useState([]);

  const onChangeSido = (e) => {
    setFilter({
      ...filter,
      sido: e.target.value
    });
  }

  const onChangeSigungu = (e) => {
    setFilter({
      ...filter,
      sigungu: e.target.value
    });
  }

  useEffect(() => {
    let filteredZones = [];
    if( filter.sido === "" && filter.sigungu === "" ){
      filteredZones = [...allZones.slice(0, 10)];
    }else{
      filteredZones = [...allZones.filter( (zone) => {
        return zone.state.includes(filter.sido) && zone.name.includes(filter.sigungu);
      }).slice(0, 10)];
    }

    setZones(filteredZones);
  }, [filter]);

  const fetchZones = async (weatherApi) => {
    
    let requestUrl = `${weatherApi}/zones`;
    if( weatherApi === undefined || weatherApi === null || weatherApi === "" ){
      requestUrl = "http://localhost:5271/zones";
    }

    const weather = await fetch(requestUrl);
    const weatherJson = await weather.json();

    const topZones = [...weatherJson.slice(0, 10)];

    setAllZones(weatherJson);
    setZones(topZones);

    console.groupCollapsed("fetchZones");
    console.log(requestUrl);
    console.log(weatherJson);
    console.log(topZones);
    console.groupEnd();
  };

  useEffect(() => {
    fetchZones(weatherApi);
  }, [weatherApi]);

  const clickZone = (zone) => {
    fetchForecasts(zone.key, zone.x, zone.y);
  }

  const fetchForecasts = async (zoneId, x, y) => {
    
    let baseUrl = `${weatherApi}`;
    if( weatherApi === undefined || weatherApi === null || weatherApi === "" ){
      baseUrl = "http://localhost:5271";
    }

    let requestUrl = `${baseUrl}/forecast/${zoneId}/${x}/${y}`;

    const forecasts = await fetch(requestUrl);
    const forecastsJson = await forecasts.json();
    setForecasts(forecastsJson);

    console.groupCollapsed("forecasts");
    console.log(baseUrl);
    console.log(requestUrl);
    console.log(forecastsJson);
    console.groupEnd();
  };

  useEffect(() => {
  }, [forecasts]);

  return (
    <div className="App">
      <h2>지역별 단기 날씨 예보 조회 서비스</h2>
      <h3>알고 싶은 지역을 선택하시면 날씨 예보를 조회할 수 있는 서비스입니다.</h3>
      
      <div>
    		<p>시군구 이름을 클릭하면 <a href="https://www.data.go.kr/tcs/dss/selectApiDataDetailView.do?publicDataPk=15084084#/tab_layer_detail_function" target="_blank">기상청 단기예보 조회서비스</a>를 통해서 하단에 단기 날씨 예보를 표시합니다.</p>
    		<p>그리드의 열 헤더를 사용하여 사용 가능한 기상 예보 구역 목록을 정렬하고 필터링할 수 있습니다.</p>
    		<p>공공데이터포털에서 활용신청을 통해 해당 서비스를 사용하실 수 있습니다.</p>
    		<p></p>
    	</div>

      <div>
        <table>
          <tbody>
            <tr>
              <th>광역시도</th>
              <th>시군구</th>
            </tr>
              {
                zones.map( (zone, index) => {
                  return(
                  <tr key={index} onClick={() => clickZone(zone) }>
                    <td>{zone.state}</td>
                    <td>{zone.name}</td>
                  </tr>
                 )} )
              }
          </tbody>
        </table>
      </div>

      <div style={{margin: 10}}>
        <label> 광역시도 필터 </label>
        <input type="text" placeholder="광역시도" onChange={onChangeSido}/>
      </div>
      <div style={{margin: 10}}>
      <label> 시군구 필터 </label>
        <input type="text" placeholder="시군구 행정구역" onChange={onChangeSigungu} />
      </div>

      {
      forecasts.length == 0 ? '' : 
      (<div>
        <p>단기 예보</p>
        <table>
          <tbody>
            <tr>
              <th>시간</th>
              <th>예보</th>
            </tr>
              {
                forecasts.map( (forecast, index) => {
                  return(
                  <tr key={index} >
                    <td>{forecast.name}</td>
                    <td>{forecast.detailedForecast}</td>
                  </tr>
                 )} )
              }
          </tbody>
        </table>
      </div>)
      }
    </div>
  );
}

export default App;
