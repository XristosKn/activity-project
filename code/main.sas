/* main.sas */
/* Activity Project - First SAS program */

options nodate nonumber;

data work.activity_example;
    input participant_id activity $ duration_minutes;
    datalines;
1 Running 30
2 Swimming 45
3 Cycling 60
;
run;

proc print data=work.activity_example;
    title "Activity Example Dataset";
run;
