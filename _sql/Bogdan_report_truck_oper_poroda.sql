

WITH st AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      st.taskdate shiftdate,
                      st.shift    shiftnum
           FROM       shifttasks st
           inner join dispatcher.dumptrucks d
           ON         d.vehid = st.vehid
           AND        d.columnnum=1
           WHERE      ((
                                            st.taskdate = :paramDateFrom
                                 AND        st.shift >= :paramShiftFrom)
                      OR         (
                                            st.taskdate > :paramDateFrom))
           AND        ((
                                            st.taskdate = :paramDateTo
                                 AND        :paramShiftTo >= st.shift)
                      OR         (
                                            :paramDateTo > st.taskdate)) ), stpgs AS
(
       SELECT sel1.tech_key,
              sel1.vehid,
              sel1.shiftdate,
              sel1.shiftnum,
              sel1.timestop,
              sel1.timego,
              sel1.poly_stop_cat_name category
       FROM   (
                         SELECT     Vehidtocontrolid(s.vehid) tech_key,
                                    s.vehid,
                                    s.shiftdate,
                                    s.shiftnum,
                                    Greatest(s.timestop, Getpredefinedtimefrom('за указанную смену', s.shiftnum,s.shiftdate)) timestop,
                                    Least(s.timego,Getpredefinedtimeto('за указанную смену', s.shiftnum,s.shiftdate))         timego,
                                    s.idlestoptype,
                                    psc.poly_stop_cat_name
                         FROM       dispatcher.shiftstoppages s
                         inner join dispatcher.poly_user_stoppages_dump ps
                         ON         ps.poly_stop_bindings_id = 23
                         AND        (
                                               ps.code = s.idlestoptype
                                    AND        ps.poly_stop_cat_id IS NOT NULL)
                         inner join dispatcher.poly_stop_categories psc
                         ON         psc.poly_stop_cat_id = ps.poly_stop_cat_id
                         WHERE      Nvl(s.idlestoptype,0) NOT IN(0,1,67)
                         AND        (
                                               s.timego - s.timestop) * 24 * 60 >=4
                         AND        s.timestop IS NOT NULL
                         AND        s.timego IS NOT NULL
                         AND        psc.poly_stop_cat_name IS NOT NULL
                         AND        ((
                                                          shiftdate = :paramDateFrom
                                               AND        shiftnum >= :paramShiftFrom)
                                    OR         (
                                                          shiftdate > :paramDateFrom))
                         AND        ((
                                                          shiftdate = :paramDateTo
                                               AND        :paramShiftTo >= shiftnum)
                                    OR         (
                                                          :paramDateTo > shiftdate )) )sel1 ), stpgsemerg AS
(
         SELECT   sel1.tech_key,
                  sel1.vehid,
                  sel1.shiftdate,
                  sel1.shiftnum,
                  SUM((sel1.timego-sel1.timestop)*24) emergidle
         FROM     (
                             SELECT     Vehidtocontrolid(s.vehid) tech_key,
                                        s.vehid,
                                        s.shiftdate,
                                        s.shiftnum,
                                        Greatest(s.timestop, Getpredefinedtimefrom('за указанную смену', s.shiftnum,s.shiftdate)) timestop,
                                        Least(s.timego,Getpredefinedtimeto('за указанную смену', s.shiftnum,s.shiftdate))         timego,
                                        s.idlestoptype,
                                        psc.poly_stop_cat_name
                             FROM       dispatcher.shiftstoppages s
                             inner join dispatcher.poly_user_stoppages_dump ps
                             ON         ps.poly_stop_bindings_id = 23
                             AND        (
                                                   ps.code = s.idlestoptype
                                        AND        ps.poly_stop_cat_id IS NOT NULL)
                             inner join dispatcher.poly_stop_categories psc
                             ON         psc.poly_stop_cat_id = ps.poly_stop_cat_id
                             inner join userstoppagetypes ust
                             ON         ust.code=ps.code
                             AND        ust.isrepair=1
                             WHERE      Nvl(s.idlestoptype,0) NOT IN(0,1,67)
                             AND        (
                                                   s.timego - s.timestop) * 24 * 60 >=4
                             AND        s.timestop IS NOT NULL
                             AND        s.timego IS NOT NULL
                             AND        psc.poly_stop_cat_name IS NOT NULL
                             AND        ((
                                                              shiftdate = :paramDateFrom
                                                   AND        shiftnum >= :paramShiftFrom)
                                        OR         (
                                                              shiftdate > :paramDateFrom))
                             AND        ((
                                                              shiftdate = :paramDateTo
                                                   AND        :paramShiftTo >= shiftnum)
                                        OR         (
                                                              :paramDateTo > shiftdate )) )sel1
         GROUP BY sel1.tech_key,
                  sel1.vehid,
                  sel1.shiftdate,
                  sel1.shiftnum ), prgq AS
(
       SELECT Vehidtocontrolid(t.vehid) tech_key,
              t.vehid,
              t.shiftdate,
              t.shiftnum,
              Greatest(t.timebegin, Getpredefinedtimefrom('за указанную смену',t.shiftnum,t.shiftdate)) timebegin,
              Least(t.timeend, Getpredefinedtimeto('за указанную смену', t.shiftnum,t.shiftdate))       timeend,
              Nvl(t.duration,0)*24                                                                                      transdur
       FROM   transitions t
       WHERE  t.transtype IN (6,7)
       AND    t.length>0
       AND    t.duration>0
       AND    15>=t.duration*24*60
       AND    t.timebegin IS NOT NULL
       AND    t.timeend IS NOT NULL
       AND    ((
                            t.shiftdate = :paramDateFrom
                     AND    t.shiftnum >= :paramShiftFrom)
              OR     (
                            t.shiftdate > :paramDateFrom))
       AND    ((
                            t.shiftdate = :paramDateTo
                     AND    :paramShiftTo >= t.shiftnum)
              OR     (
                            :paramDateTo > t.shiftdate )) ), peregemerg AS
(
          SELECT    sel1.tech_key,
                    sel1.vehid,
                    sel1.shiftdate,
                    sel1.shiftnum,
                    'перегон на ремонт' category,
                    CASE
                              WHEN Nvl(stpgsemerg.emergidle,0)=0 THEN 0
                              ELSE sel1.duration
                    END duration
          FROM      (
                             SELECT   tech_key,
                                      vehid,
                                      shiftdate,
                                      shiftnum,
                                      SUM(transdur) duration
                             FROM     prgq
                             GROUP BY tech_key,
                                      vehid,
                                      shiftdate,
                                      shiftnum)sel1
          left join stpgsemerg
          ON        stpgsemerg.tech_key=sel1.tech_key
          AND       stpgsemerg.shiftdate=sel1.shiftdate
          AND       stpgsemerg.shiftnum=sel1.shiftnum ), s AS
(
         SELECT   tech_key,
                  vehid,
                  shiftdate,
                  shiftnum,
                  category,
                  SUM((timego-timestop)*24) TIME
         FROM     stpgs
         GROUP BY tech_key,
                  vehid,
                  shiftdate,
                  shiftnum,
                  category
         UNION ALL
         SELECT   tech_key,
                  vehid,
                  shiftdate,
                  shiftnum,
                  category,
                  SUM(duration) TIME
         FROM     peregemerg
         GROUP BY tech_key,
                  vehid,
                  shiftdate,
                  shiftnum,
                  category ), totalstop AS
(
         SELECT   tech_key,
                  shiftdate,
                  shiftnum,
                  SUM(TIME) total_idle
         FROM     s
         GROUP BY tech_key,
                  shiftdate,
                  shiftnum ), psub AS
(
       SELECT *
       FROM   s pivot (SUM(TIME) FOR category IN ( 'ТР' tr,
                                                  'Т1,Т2,Т3,Т4,Т5' service,
                                                  'КР' kr,
                                                  'Обед' dinner,
                                                  'Прием/передача смены' breaks,
                                                  'ЕТО' eto,
                                                  'заправка(ДТ,вода)' refuel,
                                                  'перегоны' relocation,
                                                  'личные нужды' pers_need,
                                                  'перемещение по блоку' move_block,
                                                  'ожидание погрузки' wait_load,
                                                  'ожидание разгрузки' wait_unload,
                                                  'планировка подъездов/разбивка блока' porch_plan,
                                                  'работа бульдозера' aux_work,
                                                  'чистка ковшей/кузовов' body_clean,
                                                  'ВЗРЫВНЫЕ РАБОТЫ' vr,
                                                  'ТЕХНИЧЕСКИЙ ПЕРЕРЫВ' techper,
                                                  'Климатические условия' weather,
                                                  'Рем.элек.оборуд.' electrical,
                                                  'ДВС' dvs,
                                                  'Трансмиссия' transmission,
                                                  'Ходовая часть' chassis,
                                                  'Навесное оборудование' hinge,
                                                  'ремонт а/ш' tires,
                                                  'Гидравлическая часть' hydraulic,
                                                  'перегон на ремонт' reloc_repair,
                                                  'Наладочные работы' adjustment,
                                                  'Аварийные прочие' emerg_others,
                                                  'отсутствие вспомогательной техники' aux_lack,
                                                  'Отсут.зап.частей' parts_lack,
                                                  'Прочие' others_reason,
                                                  'доливка масла/антифриза' topp_oil,
                                                  'Остановка контралирующими органами' reg_auth,
                                                  'Отсутствие диз.топлива' fuel_lack,
                                                  'Работа с маркшейдерами, отстой а/с.' surv_work,
                                                  'Работа с геологами, отстой а/с.' geo_work,
                                                  'Очистка ходовой базы' go_base,
                                                  'переэкскавация без отгрузки' excav_nounload,
                                                  'отсутствие оператора' staff_lack,
                                                  'разборка отказов' breakdown,
                                                  'Дренажные работы без отгрузки' drainage,
                                                  'заправка экскаватора' shov_refuel,
                                                  'Резерв (ремонт экскаватора)' reserve_shov,
                                                  'Организац.прочие' org_others,
                                                  'Отсутствие экипажа (без бригады)' crew_lack,
                                                  'резерв (без учета резерва ремонта экскаватора)' reserve_noshov,
                                                  'Отсут.фронта работ' work_lack )) ), wtcq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.trips, 0)      trips
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(selres.tripnumbermanual) trips
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 201
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), wtc AS
(
       SELECT *
       FROM   wtcq pivot (SUM(trips) FOR category IN ( 'ПРС в контуре карьера' wtc_prs,
                                                      'вскрыша скальная' wtc_rockstrip,
                                                      'вскрыша рыхлая' wtc_loosestrip,
                                                      'вскрыша транзитная' wtc_transstrip,
                                                      'руда скальная' wtc_rockore,
                                                      'руда рыхлая' wtc_looseore,
                                                      'руда транзитная' wtc_transore,
                                                      'щебень' wtc_macadam,
                                                      'ВСП' wtc_iwt,
                                                      'ВКП' wtc_ipt,
                                                      'снег' wtc_snow,
                                                      'ПРС вне контура карьера' wtc_prsoutcont )) ), wrasq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.length, 0)     length
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) length
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 202
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), wras AS
(
       SELECT *
       FROM   wrasq pivot (SUM(length) FOR category IN ( 'ПРС в контуре карьера' wras_prs,
                                                        'вскрыша скальная' wras_rockstrip,
                                                        'вскрыша рыхлая' wras_loosestrip,
                                                        'вскрыша транзитная' wras_transstrip,
                                                        'руда скальная' wras_rockore,
                                                        'руда рыхлая' wras_looseore,
                                                        'руда транзитная' wras_transore,
                                                        'щебень' wras_macadam,
                                                        'ВСП' wras_iwt,
                                                        'ВКП' wras_ipt,
                                                        'снег' wras_snow,
                                                        'ПРС вне контура карьера' wras_prsoutcont )) ), twt AS
(
         SELECT   tech_key,
                  shiftdate,
                  shiftnum,
                  12-SUM(total_idle) totalworktime
         FROM     totalstop
         GROUP BY tech_key,
                  shiftdate,
                  shiftnum ), wtbt AS
(
       SELECT sel.tech_key,
              sel.vehid,
              sel.shiftdate,
              sel.shiftnum, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_prs,0)/nvl(sel.wtsum,0)
              END                          * nvl(sel.totalworktime,0) ) wt_prs, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_rockstrip,0)/nvl(sel.wtsum,0)
              END                                * nvl(sel.totalworktime,0) ) wt_rockstrip, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_loosestrip,0)/nvl(sel.wtsum,0)
              END                                 * nvl(sel.totalworktime,0) ) wt_loosestrip, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_transstrip,0)/nvl(sel.wtsum,0)
              END                                 * nvl(sel.totalworktime,0) ) wt_transstrip, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_rockore,0)/nvl(sel.wtsum,0)
              END                              * nvl(sel.totalworktime,0) ) wt_rockore, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_looseore,0)/nvl(sel.wtsum,0)
              END                               * nvl(sel.totalworktime,0) ) wt_looseore, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_transore,0)/nvl(sel.wtsum,0)
              END                               * nvl(sel.totalworktime,0) ) wt_transore, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_macadam,0)/nvl(sel.wtsum,0)
              END                              * nvl(sel.totalworktime,0) ) wt_macadam, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_iwt,0)/nvl(sel.wtsum,0)
              END                          * nvl(sel.totalworktime,0) ) wt_iwt, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_ipt,0)/nvl(sel.wtsum,0)
              END                          * nvl(sel.totalworktime,0) ) wt_ipt, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_snow,0)/nvl(sel.wtsum,0)
              END                           * nvl(sel.totalworktime,0) ) wt_snow, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_prsoutcont,0)/nvl(sel.wtsum,0)
              END                                 * nvl(sel.totalworktime,0) ) wt_prsoutcont,
              sel.totalworktime
       FROM   (
                        SELECT    wtc.tech_key,
                                  wtc.vehid,
                                  wtc.shiftdate,
                                  wtc.shiftnum,
                                  wtc.wtc_prs                                                                                                                                                                                                        wtc_prs,
                                  wtc.wtc_rockstrip                                                                                                                                                                                                        wtc_rockstrip,
                                  wtc.wtc_loosestrip                                                                                                                                                                                                        wtc_loosestrip,
                                  wtc.wtc_transstrip                                                                                                                                                                                                        wtc_transstrip,
                                  wtc.wtc_rockore                                                                                                                                                                                                        wtc_rockore,
                                  wtc.wtc_looseore                                                                                                                                                                                                        wtc_looseore,
                                  wtc.wtc_transore                                                                                                                                                                                                        wtc_transore,
                                  wtc.wtc_macadam                                                                                                                                                                                                        wtc_macadam,
                                  wtc.wtc_iwt                                                                                                                                                                                                        wtc_iwt,
                                  wtc.wtc_ipt                                                                                                                                                                                                        wtc_ipt,
                                  wtc.wtc_snow                                                                                                                                                                                                        wtc_snow,
                                  wtc.wtc_prsoutcont                                                                                                                                                                                                        wtc_prsoutcont,
                                  wras.wras_prs                                                                                                                                                                                                        wras_prs,
                                  wras.wras_rockstrip                                                                                                                                                                                                        wras_rockstrip,
                                  wras.wras_loosestrip                                                                                                                                                                                                        wras_loosestrip,
                                  wras.wras_transstrip                                                                                                                                                                                                        wras_transstrip,
                                  wras.wras_rockore                                                                                                                                                                                                        wras_rockore,
                                  wras.wras_looseore                                                                                                                                                                                                        wras_looseore,
                                  wras.wras_transore                                                                                                                                                                                                        wras_transore,
                                  wras.wras_macadam                                                                                                                                                                                                        wras_macadam,
                                  wras.wras_iwt                                                                                                                                                                                                        wras_iwt,
                                  wras.wras_ipt                                                                                                                                                                                                        wras_ipt,
                                  wras.wras_snow                                                                                                                                                                                                        wras_snow,
                                  wras.wras_prsoutcont                                                                                                                                                                                                        wras_prsoutcont,
                                  twt.totalworktime                                                                                                                                                                                                        totalworktime,
                                  nvl(wtc.wtc_prs, 0)        * nvl(wras.wras_prs, 0)                                                                                                                                                                                                        wt_prs,
                                  nvl(wtc.wtc_rockstrip, 0)  * nvl(wras.wras_rockstrip, 0)                                                                                                                                                                                                        wt_rockstrip,
                                  nvl(wtc.wtc_loosestrip, 0) * nvl(wras.wras_loosestrip, 0)                                                                                                                                                                                                        wt_loosestrip,
                                  nvl(wtc.wtc_transstrip, 0) * nvl(wras.wras_transstrip, 0)                                                                                                                                                                                                        wt_transstrip,
                                  nvl(wtc.wtc_rockore, 0)    * nvl(wras.wras_rockore, 0)                                                                                                                                                                                                        wt_rockore,
                                  nvl(wtc.wtc_looseore, 0)   * nvl(wras.wras_looseore, 0)                                                                                                                                                                                                        wt_looseore,
                                  nvl(wtc.wtc_transore, 0)   * nvl(wras.wras_transore, 0)                                                                                                                                                                                                        wt_transore,
                                  nvl(wtc.wtc_macadam, 0)    * nvl(wras.wras_macadam, 0)                                                                                                                                                                                                        wt_macadam,
                                  nvl(wtc.wtc_iwt, 0)        * nvl(wras.wras_iwt, 0)                                                                                                                                                                                                        wt_iwt,
                                  nvl(wtc.wtc_ipt, 0)        * nvl(wras.wras_ipt, 0)                                                                                                                                                                                                        wt_ipt,
                                  nvl(wtc.wtc_snow, 0)       * nvl(wras.wras_snow, 0)                                                                                                                                                                                                        wt_snow,
                                  nvl(wtc.wtc_prsoutcont, 0) * nvl(wras.wras_prsoutcont, 0)                                                                                                                                                                                                        wt_prsoutcont,
                                  nvl(wtc.wtc_prs, 0)        * nvl(wras.wras_prs, 0)+ nvl(wtc.wtc_rockstrip, 0) * nvl(wras.wras_rockstrip, 0)+ nvl(wtc.wtc_loosestrip, 0) * nvl(wras.wras_loosestrip, 0)+ nvl(wtc.wtc_transstrip, 0) * nvl(wras.wras_transstrip, 0)+ nvl(wtc.wtc_rockore, 0) * nvl(wras.wras_rockore, 0)+ nvl(wtc.wtc_looseore, 0) * nvl(wras.wras_looseore, 0)+ nvl(wtc.wtc_transore, 0) * nvl(wras.wras_transore, 0)+ nvl(wtc.wtc_macadam, 0) * nvl(wras.wras_macadam, 0)+ nvl(wtc.wtc_iwt, 0) * nvl(wras.wras_iwt, 0)+ nvl(wtc.wtc_ipt, 0) * nvl(wras.wras_ipt, 0)+ nvl(wtc.wtc_snow, 0) * nvl(wras.wras_snow, 0)+ nvl(wtc.wtc_prsoutcont, 0) * nvl(wras.wras_prsoutcont, 0) wtsum
                        FROM      wtc
                        left join wras
                        ON        wtc.tech_key=wras.tech_key
                        AND       wtc.shiftdate=wras.shiftdate
                        AND       wtc.shiftnum=wras.shiftnum
                        left join twt
                        ON        wtc.tech_key=twt.tech_key
                        AND       wtc.shiftdate=twt.shiftdate
                        AND       wtc.shiftnum=twt.shiftnum )sel ), wt AS
(
       SELECT tech_key,
              vehid,
              shiftdate,
              shiftnum,
              nvl(wt_rockstrip, 0) + nvl(wt_rockore, 0)                         wt_rockgm,
              nvl(wt_prs, 0)       + nvl(wt_loosestrip, 0)+ nvl(wt_looseore, 0) wt_loosegm,
              nvl(wt_transstrip, 0)+ nvl(wt_transore, 0)                        wt_transgm,
              0                                                                 wt_equiptrans,
              nvl(wt_macadam, 0)                                                wt_macadam,
              nvl(wt_iwt, 0)                                                    wt_iwt,
              nvl(wt_ipt, 0)                                                    wt_ipt,
              nvl(wt_snow, 0)                                                   wt_snow,
              nvl(wt_prsoutcont, 0)                                             wt_prsoutcont
       FROM   wtbt ), p AS
(
       SELECT psub.tech_key,
              psub.vehid,
              psub.shiftdate,
              psub.shiftnum,
              psub.tr,
              psub.service,
              psub.kr,
              psub.dinner,
              psub.breaks,
              psub.eto,
              psub.refuel,
              0 relocation,
              psub.pers_need,
              psub.move_block,
              psub.wait_load,
              psub.wait_unload,
              psub.porch_plan,
              psub.aux_work,
              psub.body_clean,
              psub.vr,
              psub.techper,
              psub.weather,
              psub.electrical,
              psub.dvs,
              psub.transmission,
              psub.chassis,
              psub.hinge,
              psub.tires,
              psub.hydraulic,
              psub.reloc_repair,
              psub.adjustment,
              psub.emerg_others,
              psub.aux_lack,
              psub.parts_lack,
              psub.others_reason,
              psub.topp_oil,
              psub.reg_auth,
              psub.fuel_lack,
              psub.surv_work,
              psub.geo_work,
              psub.go_base,
              psub.excav_nounload,
              psub.staff_lack,
              psub.breakdown,
              psub.drainage,
              psub.shov_refuel,
              psub.reserve_shov,
              psub.org_others,
              psub.crew_lack,
              psub.reserve_noshov,
              psub.work_lack,
              nvl(psub.tr, 0)        + nvl(psub.service, 0)+ nvl(psub.kr, 0)+ nvl(psub.dinner, 0)+ nvl(psub.breaks, 0)+ nvl(psub.eto, 0)+ nvl(psub.refuel, 0)+ nvl(psub.pers_need, 0)+ nvl(psub.move_block, 0)+ nvl(psub.wait_load, 0)+ nvl(psub.wait_unload, 0)+ nvl(psub.porch_plan, 0)+ nvl(psub.aux_work, 0)+ nvl(psub.body_clean, 0)+ nvl(psub.vr, 0)+ nvl(psub.techper, 0)                                                                                                                                                                                                        regnorm,
              nvl(psub.tr, 0)        + nvl(psub.service, 0)+ nvl(psub.kr, 0)                                                                                                                                                                                                        itogplanrem,
              nvl(psub.dinner, 0)    + nvl(psub.breaks, 0)+ nvl(psub.eto, 0)+ nvl(psub.refuel, 0)+ nvl(psub.pers_need, 0)+ nvl(psub.move_block, 0)+ nvl(psub.wait_load, 0)+ nvl(psub.wait_unload, 0)+ nvl(psub.porch_plan, 0)+ nvl(psub.aux_work, 0)+ nvl(psub.body_clean, 0)+ nvl(psub.vr, 0)+ nvl(psub.techper, 0)                                                                                                                                                                                                        itogtechnol,
              nvl(psub.electrical, 0)+ nvl(psub.dvs, 0)+ nvl(psub.transmission, 0)+ nvl(psub.chassis, 0)+ nvl(psub.hinge, 0)+ nvl(psub.tires, 0)+ nvl(psub.hydraulic, 0)+ nvl(psub.reloc_repair, 0)+ nvl(psub.adjustment, 0)+ nvl(psub.emerg_others, 0)+ nvl(psub.parts_lack, 0)+ nvl(psub.others_reason, 0)+ nvl(psub.topp_oil, 0)                                                                                                                                                                                                        itogemerg,
              nvl(psub.reg_auth, 0)  + nvl(psub.fuel_lack, 0)+ nvl(psub.surv_work, 0)+ nvl(psub.geo_work, 0)+ nvl(psub.go_base, 0)+ nvl(psub.excav_nounload, 0)+ nvl(psub.staff_lack, 0)+ nvl(psub.breakdown, 0)+ nvl(psub.drainage, 0)+ nvl(psub.shov_refuel, 0)+ nvl(psub.reserve_shov, 0)+ nvl(psub.org_others, 0)+ nvl(psub.crew_lack, 0)+ nvl(psub.reserve_noshov, 0)+ nvl(psub.work_lack, 0)                                                                                                                                                                                                        itogorg,
              nvl(psub.dinner, 0)    + nvl(psub.breaks, 0)+ nvl(psub.eto, 0)+ nvl(psub.refuel, 0)+ nvl(psub.pers_need, 0)+ nvl(psub.move_block, 0)+ nvl(psub.wait_load, 0)+ nvl(psub.wait_unload, 0)+ nvl(psub.porch_plan, 0)+ nvl(psub.aux_work, 0)+ nvl(psub.body_clean, 0)+ nvl(psub.vr, 0)+ nvl(psub.techper, 0)+ nvl(psub.weather, 0)+ nvl(psub.reg_auth, 0)+ nvl(psub.fuel_lack, 0)+ nvl(psub.surv_work, 0)+ nvl(psub.geo_work, 0)+ nvl(psub.go_base, 0)+ nvl(psub.excav_nounload, 0)+ nvl(psub.staff_lack, 0)+ nvl(psub.breakdown, 0)+ nvl(psub.drainage, 0)+ nvl(psub.shov_refuel, 0)+ nvl(psub.reserve_shov, 0)+ nvl(psub.org_others, 0)+ nvl(psub.crew_lack, 0)+ nvl(psub.reserve_noshov, 0)+ nvl(psub.work_lack, 0) s_kio
       FROM   psub ), gmwq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.weight, 0)     weight
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(selres.tripnumbermanual*selres.avweight)/1000 weight
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 3
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), gmw AS
(
       SELECT *
       FROM   gmwq pivot (SUM(weight) FOR category IN ( 'ПРС в контуре карьера' gmw_prs,
                                                       'вскрыша скальная' gmw_rockstrip,
                                                       'вскрыша рыхлая' gmw_loosestrip,
                                                       'вскрыша транзитная' gmw_transstrip,
                                                       'руда скальная' gmw_rockore,
                                                       'руда рыхлая' gmw_looseore,
                                                       'руда транзитная' gmw_transore )) ), gmvq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.vol, 0)        vol
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(decode(nvl(selres.avweight, 0),
                                                       0, 0,
                                                       selres.avweight*selres.tripnumbermanual/nvl(decode(selres.weightrate,
                                                                                                          0, selres.avweight,
                                                                                                          selres.weightrate), selres.avweight)*selres.volumerate))/1000 vol
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight,
                                                                  sra.weightrate,
                                                                  sra.volumerate
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 3
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), gmv AS
(
       SELECT *
       FROM   gmvq pivot (SUM(vol) FOR category IN ( 'ПРС в контуре карьера' gmv_prs,
                                                    'вскрыша скальная' gmv_rockstrip,
                                                    'вскрыша рыхлая' gmv_loosestrip,
                                                    'вскрыша транзитная' gmv_transstrip,
                                                    'руда скальная' gmv_rockore,
                                                    'руда рыхлая' gmv_looseore,
                                                    'руда транзитная' gmv_transore )) ), trq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.trips, 0)      trips
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(selres.tripnumbermanual) trips
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 4
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), tr AS
(
       SELECT *
       FROM   trq pivot (SUM(trips) FOR category IN ( 'ПРС в контуре карьера' tr_prs,
                                                     'вскрыша скальная' tr_rockstrip,
                                                     'вскрыша рыхлая' tr_loosestrip,
                                                     'вскрыша транзитная' tr_transstrip,
                                                     'по руде' tr_ore )) ), rasgmq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.length, 0)     length
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) length
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 61
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), rasgm AS
(
       SELECT *
       FROM   rasgmq pivot (SUM(length) FOR category IN ( 'ср.взв.расст.г.м.' ras_gm )) ), rasq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.length, 0)     length
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) length
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 5
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), ras AS
(
       SELECT *
       FROM   rasq pivot (SUM(length) FOR category IN ( 'ПРС в контуре карьера' ras_prs,
                                                       'вскрыша скальная' ras_rockstrip,
                                                       'вскрыша рыхлая' ras_loosestrip,
                                                       'вскрыша транзитная' ras_transstrip,
                                                       'руда скальная' ras_rockore,
                                                       'руда рыхлая' ras_looseore,
                                                       'руда транзитная' ras_transore )) ), avwq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.avweight, 0)   avweight
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avweight)/SUM(selres.tripnumbermanual)) avweight
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 6
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), avw AS
(
       SELECT *
       FROM   avwq pivot (SUM(avweight) FOR category IN ( 'ПРС в контуре карьера' avw_prs,
                                                         'по скале' avw_rock,
                                                         'по рыхлой' avw_loose,
                                                         'по транзитной' avw_trans )) ), avwgmq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.avweight, 0)   avweight
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avweight)/SUM(selres.tripnumbermanual)) avweight
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 41
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), avwgm AS
(
       SELECT *
       FROM   avwgmq pivot (SUM(avweight) FOR category IN ( 'ср.взв.загр.г.м.' avw_gm )) ), ftq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.gruzob, 0)     gruzob
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(selres.tripnumbermanual*selres.avweight)/1000 * decode(SUM(selres.tripnumbermanual),
                                                                                                       0, 0,
                                                                                                       SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) gruzob
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 21
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), ft AS
(
       SELECT *
       FROM   ftq pivot (SUM(gruzob) FOR category IN ( 'ПРС в контуре карьера' ft_prs,
                                                      'вскрыша скальная' ft_rockstrip,
                                                      'вскрыша рыхлая' ft_loosestrip,
                                                      'вскрыша транзитная' ft_transstrip,
                                                      'руда скальная' ft_rockore,
                                                      'руда рыхлая' ft_looseore,
                                                      'руда транзитная' ft_transore )) ), dtmh AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      nvl(sel.mh, 0) mh
           FROM       (
                               SELECT   selres.vehid,
                                        SUM(selres.motohoursend-selres.motohoursbegin) mh
                               FROM     (
                                               SELECT vehid,
                                                      taskdate,
                                                      shift,
                                                      motohoursbegin,
                                                      motohoursend
                                               FROM   shiftlensandtimes
                                               WHERE  ((
                                                                    taskdate = :paramDateFrom
                                                             AND    shift >= :paramShiftFrom)
                                                      OR     (
                                                                    taskdate > :paramDateFrom))
                                               AND    ((
                                                                    taskdate = :paramDateTo
                                                             AND    :paramShiftTo >= shift)
                                                      OR     (
                                                                    :paramDateTo > taskdate )) )selres
                               GROUP BY selres.vehid)sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), norep AS
(
       SELECT vehidtocontrolid(vehid) tech_key,
              vehid,
              taskdate shiftdate,
              shift    shiftnum
       FROM   kgp_noreptech
       WHERE  ((
                            taskdate = :paramDateFrom
                     AND    shift >= :paramShiftFrom)
              OR     (
                            taskdate > :paramDateFrom))
       AND    ((
                            taskdate = :paramDateTo
                     AND    :paramShiftTo >= shift)
              OR     (
                            :paramDateTo > taskdate )) )
SELECT    'Автосамосвалы' category,
          smsv.model                   model,
          position,
          tech_id,
          /* вскрыша транзитная м3 */
          round(SUM((gmv_transstrip)) * 1000, 3)                                                AS val_tr,
          round(nvl(SUM(ras_transstrip * gmv_transstrip) / nullif(SUM(gmv_transstrip),0),0), 3) AS len_tr,
          /* вскрыша транзитная км */
          /* вскрыша скальная м3 */
          round(SUM((gmv_rockstrip)) * 1000, 3)                                              AS val_sk,
          round(nvl(SUM(ras_rockstrip * gmv_rockstrip) / nullif(SUM(gmv_rockstrip),0),0), 3) AS len_sk,
          /* вскрыша скальная км */
          /* вскрыша рыхлая м3 */
          round(SUM((gmv_loosestrip)) * 1000, 3)                                                AS val_rih,
          round(nvl(SUM(ras_loosestrip * gmv_loosestrip) / nullif(SUM(gmv_loosestrip),0),0), 3) AS len_rih,
          /* вскрыша рыхлая км */
          /* ПРС м3 */
          round(SUM((gmv_prs)) * 1000, 3)                                  AS val_prs,
          round(nvl(SUM(ras_prs * gmv_prs) / nullif(SUM(gmv_prs),0),0), 3) AS len_prs,
          /* ПРС км */
          /* руда м3 */
          round(SUM(gmv_rockore + gmv_looseore + gmv_transore) * 1000, 3)                                                                                                                                                                                                        AS val_rud,
          round( nvl(( SUM(gmv_rockore) * nvl(SUM(ras_rockore * gmv_rockore) / nullif(SUM(gmv_rockore),0), 0) + SUM(gmv_looseore) * nvl(SUM(ras_looseore * gmv_looseore) / nullif(SUM(gmv_looseore),0), 0) + SUM(gmv_transore) * nvl(SUM(ras_transore * gmv_transore) / nullif(SUM(gmv_transore),0), 0) ) / nullif(SUM( gmv_rockore + gmv_looseore + gmv_transore ), 0), 0) , 3) AS len_rud,
          /* руда км */
          /* transore м3 */
          round(SUM((gmv_transore)) * 1000, 3)                                             AS val_transore,
          round(nvl(SUM(ras_transore * gmv_transore) / nullif(SUM(gmv_transore),0), 0), 3) AS len_transore,
          /* transore км */
          /* rockore м3 */
          round(SUM((gmv_rockore)) * 1000, 3)                                           AS val_rockore,
          round(nvl(SUM(ras_rockore * gmv_rockore) / nullif(SUM(gmv_rockore),0), 0), 3) AS len_rockore,
          /* rockore км */
          /* looseore м3 */
          round(SUM((gmv_looseore)) * 1000, 3)                                             AS val_looseore,
          round(nvl(SUM(ras_looseore * gmv_looseore) / nullif(SUM(gmv_looseore),0), 0), 3) AS len_looseore
          /* looseore км */
FROM      (
                     SELECT     pt.position,
                                t.vehid tech_id,
                                st.shiftdate,
                                st.shiftnum,
                                nvl(dtmh.mh, 0) mh,
                                CASE
                                           WHEN nvl(norep.tech_key,0)=0 THEN 12
                                           ELSE NULL
                                END                                                                                                                                                                                                        kalentime,
                                nvl(wt.wt_rockgm,0)+ nvl(wt.wt_loosegm,0)+ nvl(wt.wt_transgm,0)+ nvl(wt.wt_equiptrans,0)+ nvl(wt.wt_macadam,0)+ nvl(wt.wt_iwt,0)+ nvl(wt.wt_ipt,0)+ nvl(wt.wt_snow,0)+ nvl(wt.wt_prsoutcont,0)+ nvl(p.dinner,0)+ nvl(p.breaks,0)+ nvl(p.eto,0)+ nvl(p.refuel,0)+ nvl(p.relocation,0)+ nvl(p.pers_need,0)+ nvl(p.move_block,0)+ nvl(p.wait_load,0)+ nvl(p.wait_unload,0)+ nvl(p.porch_plan,0)+ nvl(p.aux_work,0)+ nvl(p.body_clean,0)+ nvl(p.vr,0)+ nvl(p.techper,0) chvhoz,
                                nvl(wt.wt_rockgm,0)+ nvl(wt.wt_loosegm,0)+ nvl(wt.wt_transgm,0)+ nvl(wt.wt_equiptrans,0)+ nvl(wt.wt_macadam,0)+ nvl(wt.wt_iwt,0)+ nvl(wt.wt_ipt,0)+ nvl(wt.wt_snow,0)+ nvl(wt.wt_prsoutcont,0)                                                                                                                                                                                                        wt_all,
                                nvl(wt.wt_rockgm,0)+ nvl(wt.wt_loosegm,0)+ nvl(wt.wt_transgm,0)                                                                                                                                                                                                        wt_gm,
                                nvl(wt.wt_rockgm,0)                                                                                                                                                                                                        wt_rockgm,
                                nvl(wt.wt_loosegm,0)                                                                                                                                                                                                        wt_loosegm,
                                nvl(wt.wt_transgm,0)                                                                                                                                                                                                        wt_transgm,
                                nvl(wt.wt_equiptrans,0)+ nvl(wt.wt_macadam,0)+ nvl(wt.wt_iwt,0)+ nvl(wt.wt_ipt,0)+ nvl(wt.wt_snow,0)+ nvl(wt.wt_prsoutcont,0)                                                                                                                                                                                                        wt_ew,
                                nvl(wt.wt_equiptrans,0)                                                                                                                                                                                                        wt_equiptrans,
                                nvl(wt.wt_macadam,0)                                                                                                                                                                                                        wt_macadam,
                                nvl(wt.wt_iwt,0)                                                                                                                                                                                                        wt_iwt,
                                nvl(wt.wt_ipt,0)                                                                                                                                                                                                        wt_ipt,
                                nvl(wt.wt_snow,0)                                                                                                                                                                                                        wt_snow,
                                nvl(wt.wt_prsoutcont,0)                                                                                                                                                                                                        wt_prsoutcont,
                                nvl(p.regnorm,0)                                                                                                                                                                                                        regnorm,
                                nvl(p.itogplanrem,0)                                                                                                                                                                                                        itogplanrem,
                                nvl(p.tr,0)                                                                                                                                                                                                        tr,
                                nvl(p.service,0)                                                                                                                                                                                                        service,
                                nvl(p.kr,0)                                                                                                                                                                                                        kr,
                                nvl(p.itogtechnol,0)                                                                                                                                                                                                        itogtechnol,
                                nvl(p.dinner,0)                                                                                                                                                                                                        dinner,
                                nvl(p.breaks,0)                                                                                                                                                                                                        breaks,
                                nvl(p.eto,0)                                                                                                                                                                                                        eto,
                                nvl(p.refuel,0)                                                                                                                                                                                                        refuel,
                                nvl(p.relocation,0)                                                                                                                                                                                                        relocation,
                                nvl(p.pers_need,0)                                                                                                                                                                                                        pers_need,
                                nvl(p.move_block,0)                                                                                                                                                                                                        move_block,
                                nvl(p.wait_load,0)                                                                                                                                                                                                        wait_load,
                                nvl(p.wait_unload,0)                                                                                                                                                                                                        wait_unload,
                                nvl(p.porch_plan,0)                                                                                                                                                                                                        porch_plan,
                                nvl(p.aux_work,0)                                                                                                                                                                                                        aux_work,
                                nvl(p.body_clean,0)                                                                                                                                                                                                        body_clean,
                                nvl(p.vr,0)                                                                                                                                                                                                        vr,
                                nvl(p.techper,0)                                                                                                                                                                                                        techper,
                                nvl(p.weather,0)                                                                                                                                                                                                        weather,
                                nvl(p.itogemerg,0)                                                                                                                                                                                                        itogemerg,
                                nvl(p.electrical,0)                                                                                                                                                                                                        electrical,
                                nvl(p.dvs,0)                                                                                                                                                                                                        dvs,
                                nvl(p.transmission,0)                                                                                                                                                                                                        transmission,
                                nvl(p.chassis,0)                                                                                                                                                                                                        chassis,
                                nvl(p.hinge,0)                                                                                                                                                                                                        hinge,
                                nvl(p.tires,0)                                                                                                                                                                                                        tires,
                                nvl(p.hydraulic,0)                                                                                                                                                                                                        hydraulic,
                                nvl(p.reloc_repair,0)                                                                                                                                                                                                        reloc_repair,
                                nvl(p.adjustment,0)                                                                                                                                                                                                        adjustment,
                                nvl(p.emerg_others,0)                                                                                                                                                                                                        emerg_others,
                                nvl(p.aux_lack,0)                                                                                                                                                                                                        aux_lack,
                                nvl(p.parts_lack,0)                                                                                                                                                                                                        parts_lack,
                                nvl(p.others_reason,0)                                                                                                                                                                                                        others_reason,
                                nvl(p.topp_oil,0)                                                                                                                                                                                                        topp_oil,
                                nvl(p.itogorg,0)                                                                                                                                                                                                        itogorg,
                                nvl(p.reg_auth,0)                                                                                                                                                                                                        reg_auth,
                                nvl(p.fuel_lack,0)                                                                                                                                                                                                        fuel_lack,
                                nvl(p.surv_work,0)                                                                                                                                                                                                        surv_work,
                                nvl(p.geo_work,0)                                                                                                                                                                                                        geo_work,
                                nvl(p.go_base,0)                                                                                                                                                                                                        go_base,
                                nvl(p.excav_nounload,0)                                                                                                                                                                                                        excav_nounload,
                                nvl(p.staff_lack,0)                                                                                                                                                                                                        staff_lack,
                                nvl(p.breakdown,0)                                                                                                                                                                                                        breakdown,
                                nvl(p.drainage,0)                                                                                                                                                                                                        drainage,
                                nvl(p.shov_refuel,0)                                                                                                                                                                                                        shov_refuel,
                                nvl(p.reserve_shov,0)                                                                                                                                                                                                        reserve_shov,
                                nvl(p.org_others,0)                                                                                                                                                                                                        org_others,
                                nvl(p.crew_lack,0)                                                                                                                                                                                                        crew_lack,
                                nvl(p.reserve_noshov,0)                                                                                                                                                                                                        reserve_noshov,
                                nvl(p.work_lack,0)                                                                                                                                                                                                        work_lack,
                                nvl(p.s_kio,0)                                                                                                                                                                                                        s_kio,
                                nvl(gmv.gmv_prs,0)+ nvl(gmv.gmv_rockstrip,0)+ nvl(gmv.gmv_loosestrip,0)+ nvl(gmv.gmv_transstrip,0)+ nvl(gmv.gmv_rockore,0)+ nvl(gmv.gmv_looseore,0)+ nvl(gmv.gmv_transore,0)                                                                                                                                                                                                        gmv_gm,
                                nvl(gmw.gmw_prs,0)+ nvl(gmw.gmw_rockstrip,0)+ nvl(gmw.gmw_loosestrip,0)+ nvl(gmw.gmw_transstrip,0)+ nvl(gmw.gmw_rockore,0)+ nvl(gmw.gmw_looseore,0)+ nvl(gmw.gmw_transore,0)                                                                                                                                                                                                        gmw_gm,
                                nvl(gmv.gmv_prs,0)                                                                                                                                                                                                        gmv_prs,
                                nvl(gmw.gmw_prs,0)                                                                                                                                                                                                        gmw_prs,
                                nvl(gmv.gmv_rockstrip,0)                                                                                                                                                                                                        gmv_rockstrip,
                                nvl(gmw.gmw_rockstrip,0)                                                                                                                                                                                                        gmw_rockstrip,
                                nvl(gmv.gmv_loosestrip,0)                                                                                                                                                                                                        gmv_loosestrip,
                                nvl(gmw.gmw_loosestrip,0)                                                                                                                                                                                                        gmw_loosestrip,
                                nvl(gmv.gmv_transstrip,0)                                                                                                                                                                                                        gmv_transstrip,
                                nvl(gmw.gmw_transstrip,0)                                                                                                                                                                                                        gmw_transstrip,
                                nvl(gmv.gmv_rockore,0)                                                                                                                                                                                                        gmv_rockore,
                                nvl(gmw.gmw_rockore,0)                                                                                                                                                                                                        gmw_rockore,
                                nvl(gmv.gmv_looseore,0)                                                                                                                                                                                                        gmv_looseore,
                                nvl(gmw.gmw_looseore,0)                                                                                                                                                                                                        gmw_looseore,
                                nvl(gmv.gmv_transore,0)                                                                                                                                                                                                        gmv_transore,
                                nvl(gmw.gmw_transore,0)                                                                                                                                                                                                        gmw_transore,
                                CASE
                                           WHEN nvl(wt.wt_rockgm,0)=0 THEN 0
                                           ELSE (nvl(gmv.gmv_rockstrip,0)+ nvl(gmv.gmv_rockore,0) )/nvl(wt.wt_rockgm,0)
                                END opv_rockgm,
                                CASE
                                           WHEN nvl(wt.wt_rockgm,0)=0 THEN 0
                                           ELSE (nvl(gmw.gmw_rockstrip,0)+ nvl(gmw.gmw_rockore,0) )/nvl(wt.wt_rockgm,0)
                                END opw_rockgm,
                                CASE
                                           WHEN nvl(wt.wt_loosegm,0)=0 THEN 0
                                           ELSE (nvl(gmv.gmv_loosestrip,0)+ nvl(gmv.gmv_looseore,0) )/nvl(wt.wt_loosegm,0)
                                END opv_loosegm,
                                CASE
                                           WHEN nvl(wt.wt_loosegm,0)=0 THEN 0
                                           ELSE (nvl(gmw.gmw_loosestrip,0)+ nvl(gmw.gmw_looseore,0) )/nvl(wt.wt_loosegm,0)
                                END opw_loosegm,
                                CASE
                                           WHEN nvl(wt.wt_transgm,0)=0 THEN 0
                                           ELSE (nvl(gmv.gmv_transstrip,0)+ nvl(gmv.gmv_transore,0) )/nvl(wt.wt_transgm,0)
                                END opv_transgm,
                                CASE
                                           WHEN nvl(wt.wt_transgm,0)=0 THEN 0
                                           ELSE (nvl(gmw.gmw_transstrip,0)+ nvl(gmw.gmw_transore,0) )/nvl(wt.wt_transgm,0)
                                END opw_transgm,
                                CASE
                                           WHEN (
                                                                 nvl(gmv.gmv_prs,0)+ nvl(gmv.gmv_rockstrip,0)+ nvl(gmv.gmv_loosestrip,0)+ nvl(gmv.gmv_transstrip,0)+ nvl(gmv.gmv_rockore,0)+ nvl(gmv.gmv_looseore,0)+ nvl(gmv.gmv_transore,0) ) = 0 THEN 0
                                           ELSE ( (nvl(gmv.gmv_rockstrip,0)+ nvl(gmv.gmv_rockore,0) )*
                                                      CASE
                                                                 WHEN nvl(wt.wt_rockgm,0)=0 THEN 0
                                                                 ELSE (nvl(gmv.gmv_rockstrip,0)+ nvl(gmv.gmv_rockore,0) )/nvl(wt.wt_rockgm,0)
                                                      END + (nvl(gmv.gmv_loosestrip,0)+ nvl(gmv.gmv_looseore,0) )*
                                                      CASE
                                                                 WHEN nvl(wt.wt_loosegm,0)=0 THEN 0
                                                                 ELSE (nvl(gmv.gmv_loosestrip,0)+ nvl(gmv.gmv_looseore,0) )/nvl(wt.wt_loosegm,0)
                                                      END + (nvl(gmv.gmv_transstrip,0)+ nvl(gmv.gmv_transore,0) )*
                                                      CASE
                                                                 WHEN nvl(wt.wt_transgm,0)=0 THEN 0
                                                                 ELSE (nvl(gmv.gmv_transstrip,0)+ nvl(gmv.gmv_transore,0) )/nvl(wt.wt_transgm,0)
                                                      END )/ ( nvl(gmv.gmv_prs,0)+ nvl(gmv.gmv_rockstrip,0)+ nvl(gmv.gmv_loosestrip,0)+ nvl(gmv.gmv_transstrip,0)+ nvl(gmv.gmv_rockore,0)+ nvl(gmv.gmv_looseore,0)+ nvl(gmv.gmv_transore,0) )
                                END avv_prod,
                                CASE
                                           WHEN (
                                                                 nvl(gmw.gmw_prs,0)+ nvl(gmw.gmw_rockstrip,0)+ nvl(gmw.gmw_loosestrip,0)+ nvl(gmw.gmw_transstrip,0)+ nvl(gmw.gmw_rockore,0)+ nvl(gmw.gmw_looseore,0)+ nvl(gmw.gmw_transore,0) ) = 0 THEN 0
                                           ELSE ( (nvl(gmw.gmw_rockstrip,0)+ nvl(gmw.gmw_rockore,0) )*
                                                      CASE
                                                                 WHEN nvl(wt.wt_rockgm,0)=0 THEN 0
                                                                 ELSE (nvl(gmw.gmw_rockstrip,0)+ nvl(gmw.gmw_rockore,0) )/nvl(wt.wt_rockgm,0)
                                                      END + (nvl(gmw.gmw_loosestrip,0)+ nvl(gmw.gmw_looseore,0) )*
                                                      CASE
                                                                 WHEN nvl(wt.wt_loosegm,0)=0 THEN 0
                                                                 ELSE (nvl(gmw.gmw_loosestrip,0)+ nvl(gmw.gmw_looseore,0) )/nvl(wt.wt_loosegm,0)
                                                      END + (nvl(gmw.gmw_transstrip,0)+ nvl(gmw.gmw_transore,0) )*
                                                      CASE
                                                                 WHEN nvl(wt.wt_transgm,0)=0 THEN 0
                                                                 ELSE (nvl(gmw.gmw_transstrip,0)+ nvl(gmw.gmw_transore,0) )/nvl(wt.wt_transgm,0)
                                                      END )/ ( nvl(gmw.gmw_prs,0)+ nvl(gmw.gmw_rockstrip,0)+ nvl(gmw.gmw_loosestrip,0)+ nvl(gmw.gmw_transstrip,0)+ nvl(gmw.gmw_rockore,0)+ nvl(gmw.gmw_looseore,0)+ nvl(gmw.gmw_transore,0) )
                                END                                                                                                          avw_prod,
                                nvl(tr.tr_prs,0)+ nvl(tr.tr_rockstrip,0)+ nvl(tr.tr_loosestrip,0)+ nvl(tr.tr_transstrip,0)+ nvl(tr.tr_ore,0) tr_gm,
                                nvl(tr.tr_prs,0)                                                                                             tr_prs,
                                nvl(tr.tr_rockstrip,0)                                                                                       tr_rockstrip,
                                nvl(tr.tr_loosestrip,0)                                                                                      tr_loosestrip,
                                nvl(tr.tr_transstrip,0)                                                                                      tr_transstrip,
                                nvl(tr.tr_ore,0)                                                                                             tr_ore,
                                nvl(rasgm.ras_gm,0)                                                                                          ras_gm,
                                nvl(ras.ras_prs,0)                                                                                           ras_prs,
                                nvl(ras.ras_rockstrip,0)                                                                                     ras_rockstrip,
                                nvl(ras.ras_loosestrip,0)                                                                                    ras_loosestrip,
                                nvl(ras.ras_transstrip,0)                                                                                    ras_transstrip,
                                nvl(ras.ras_rockore,0)                                                                                       ras_rockore,
                                nvl(ras.ras_looseore,0)                                                                                      ras_looseore,
                                nvl(ras.ras_transore,0)                                                                                      ras_transore,
                                CASE
                                           WHEN nvl(norep.tech_key,0) = 0 THEN
                                                      CASE
                                                                 WHEN (
                                                                                       12 - nvl(p.breaks,0) - nvl(p.eto,0) - nvl(p.dinner,0) ) = 0 THEN NULL
                                                                 ELSE ( ( nvl(wt.wt_rockgm,0)+ nvl(wt.wt_loosegm,0)+ nvl(wt.wt_transgm,0)+ nvl(wt.wt_equiptrans,0)+ nvl(wt.wt_macadam,0)+ nvl(wt.wt_iwt,0)+ nvl(wt.wt_ipt,0)+ nvl(wt.wt_snow,0)+ nvl(wt.wt_prsoutcont,0) ) + nvl(p.work_lack,0)+ nvl(p.itogorg,0)+ nvl(p.weather,0) )/ ( 12 - nvl(p.breaks,0) - nvl(p.eto,0) - nvl(p.dinner,0) )
                                                      END
                                           ELSE NULL
                                END koldumpline,
                                CASE
                                           WHEN nvl(norep.tech_key,0) = 0 THEN
                                                      CASE
                                                                 WHEN (
                                                                                       12  - nvl(p.breaks,0) - nvl(p.eto,0) - nvl(p.dinner,0) ) = 0 THEN NULL
                                                                 ELSE ( nvl(wt.wt_rockgm,0)+ nvl(wt.wt_loosegm,0)+ nvl(wt.wt_transgm,0)+ nvl(wt.wt_equiptrans,0)+ nvl(wt.wt_macadam,0)+ nvl(wt.wt_iwt,0)+ nvl(wt.wt_ipt,0)+ nvl(wt.wt_snow,0)+ nvl(wt.wt_prsoutcont,0) )/ ( 12 - nvl(p.breaks,0) - nvl(p.eto,0) - nvl(p.dinner,0) )
                                                      END
                                           ELSE NULL
                                END                         koldumpwork,
                                nvl(avwgm.avw_gm,0)         avw_gm,
                                nvl(avw.avw_prs,0)          avw_prs,
                                nvl(avw.avw_rock,0)         avw_rock,
                                nvl(avw.avw_loose,0)        avw_loose,
                                nvl(avw.avw_trans,0)        avw_trans,
                                nvl(totalstop.total_idle,0) total_idle,
                                CASE
                                           WHEN nvl(norep.tech_key,0)=0 THEN 12
                                           ELSE NULL
                                END- ( nvl(wt.wt_rockgm,0)+ nvl(wt.wt_loosegm,0)+ nvl(wt.wt_transgm,0)+ nvl(wt.wt_equiptrans,0)+ nvl(wt.wt_macadam,0)+ nvl(wt.wt_iwt,0)+ nvl(wt.wt_ipt,0)+ nvl(wt.wt_snow,0)+ nvl(wt.wt_prsoutcont,0)+ nvl(totalstop.total_idle,0) ) balance_time
                     FROM       dispatcher.allvehs t
                     inner join kgp.pto_tech pt
                     ON         pt.controlid = t.controlid
                     AND        pt.category='Автосамосвалы'
                     left join  st
                     ON         st.tech_key = t.controlid
                     left join  wt
                     ON         wt.tech_key = t.controlid
                     AND        wt.shiftdate=st.shiftdate
                     AND        wt.shiftnum=st.shiftnum
                     left join  p
                     ON         p.tech_key=t.controlid
                     AND        p.shiftdate=st.shiftdate
                     AND        p.shiftnum=st.shiftnum
                     left join  gmw
                     ON         gmw.tech_key=t.controlid
                     AND        gmw.shiftdate=st.shiftdate
                     AND        gmw.shiftnum=st.shiftnum
                     left join  gmv
                     ON         gmv.tech_key=t.controlid
                     AND        gmv.shiftdate=st.shiftdate
                     AND        gmv.shiftnum=st.shiftnum
                     left join  tr
                     ON         tr.tech_key=t.controlid
                     AND        tr.shiftdate=st.shiftdate
                     AND        tr.shiftnum=st.shiftnum
                     left join  rasgm
                     ON         rasgm.tech_key=t.controlid
                     AND        rasgm.shiftdate=st.shiftdate
                     AND        rasgm.shiftnum=st.shiftnum
                     left join  ras
                     ON         ras.tech_key=t.controlid
                     AND        ras.shiftdate=st.shiftdate
                     AND        ras.shiftnum=st.shiftnum
                     left join  ft
                     ON         ft.tech_key=t.controlid
                     AND        ft.shiftdate=st.shiftdate
                     AND        ft.shiftnum=st.shiftnum
                     left join  avw
                     ON         avw.tech_key=t.controlid
                     AND        avw.shiftdate=st.shiftdate
                     AND        avw.shiftnum=st.shiftnum
                     left join  avwgm
                     ON         avwgm.tech_key=t.controlid
                     AND        avwgm.shiftdate=st.shiftdate
                     AND        avwgm.shiftnum=st.shiftnum
                     left join  dtmh
                     ON         dtmh.tech_key=t.controlid
                     left join  totalstop
                     ON         totalstop.tech_key=t.controlid
                     AND        totalstop.shiftdate=st.shiftdate
                     AND        totalstop.shiftnum=st.shiftnum
                     left join  norep
                     ON         norep.tech_key=t.controlid
                     AND        norep.shiftdate=st.shiftdate
                     AND        norep.shiftnum=st.shiftnum ) src
left join
          (
                 SELECT *
                 FROM   dumptrucks
                 WHERE  columnnum=1 )smsv
ON        src.tech_id=smsv.vehid
WHERE     (
                    tech_id = :paramSelectTechId
          OR        :paramSelectTechId = 'Все')
GROUP BY  model,
          position,
          tech_id
ORDER BY  model,
          position,
          tech_id DESC		
		
		  