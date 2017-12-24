var requestService =function (url, data, successCallBack, errorCallBack,isPost=false) {
    if (isPost) {
        post(url, data, successCallBack, errorCallBack);
        } else {
            get(url,successCallBack,errorCallBack);
        }
    }

    function get(url,successCallBack,errorCallBack) {
        $.ajax({
            url: url,
            cache: false,
            dataType: "json",
            success: function (data) {
                if (data && data.returnCode && data.returnCode == "1000") {
                    alert("please log in first ");
                    window.location = "../login.html";
                    return;
                }
                successCallBack(data);
            },
            error: function () {
                if (errorCallBack) {
                    errorCallBack(data);
                }
            }
        });
    }

    function post(url, data, successCallBack, errorCallBack) {
        $.ajax({
            url: url,
            cache: false,
            dataType: "json",
            data: data,
            type: 'POST',
            success: function (data) {
                if (data && data.returnCode && data.returnCode == "1000") {
                    window.location = "../login.html";
                    return;
                } 
                successCallBack(data);
            },
            error: function () {
                if (errorCallBack) {
                    errorCallBack(data);
                }
            }
        });
    }
