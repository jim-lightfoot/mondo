<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="xml" indent="yes"/>

  <xsl:param    name="APPNAME" />
  <xsl:variable name="FONT" select="'verdana, arial, helvetica, sans-serif'" />
  
  <!-- ******************************************************************** -->
  <!-- *** Main Template                                                *** -->
  <!-- ******************************************************************** -->
  <xsl:template match="/">

    <html>
      <body style="margin: 0px">
        <table valign="top" width="100%" height="100%" cellpadding="0" cellspacing="0" border="0" style="margin: 0px">
          <tr>
            <td valign="top">
              <table valign="top" width="100%" height="10" cellpadding="10" cellspacing="0" border="0" style="margin: 0px">
                <tr>
                  <td padding="5px 5px 5px 10px" bgcolor="#151584" style="font-family: {$FONT}; font-size: 16pt; font-weight: bold; color: white">
                    Exception thrown by &quot;<xsl:value-of select="$APPNAME" />&quot;
                  </td>
                </tr>
                <tr>
                  <td>
     
                    <xsl:apply-templates />

                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </body>
    </html>

  </xsl:template>
  
  <!-- ******************************************************************** -->
  <!-- *** Exception Template                                           *** -->
  <!-- ******************************************************************** -->
  <xsl:template match="Exception">
    
    <table width="100%" cellpadding="5" cellspacing="0">
      <tr>
        <td>
          <table width="100%" cellpadding="0" cellspacing="1" bgcolor="#D0D0D0">
   
            <xsl:call-template name="ItemRow">
              <xsl:with-param name="CAPTION" select="'Message'" />
              <xsl:with-param name="VALUE"   select="Message" />
              <xsl:with-param name="HILITE"  select="1" />
            </xsl:call-template>
                  
            <xsl:call-template name="ItemRow">
              <xsl:with-param name="CAPTION" select="'Type'" />
              <xsl:with-param name="VALUE"   select="Type" />
            </xsl:call-template>
                  
            <xsl:call-template name="ItemRow">
              <xsl:with-param name="CAPTION" select="'Source'" />
              <xsl:with-param name="VALUE"   select="Source" />
            </xsl:call-template>
                  
            <xsl:call-template name="ItemRow">
              <xsl:with-param name="CAPTION" select="'Stack Trace'" />
              <xsl:with-param name="VALUE"   select="StackTrace" />
            </xsl:call-template>
                
          </table>
        </td>
      </tr>
    </table>
    
  </xsl:template>

  <!-- ******************************************************************** -->
  <!-- *** ServerInfo Template                                          *** -->
  <!-- ******************************************************************** -->
  <xsl:template match="ServerInfo">
    
   <table width="100%" cellpadding="5" cellspacing="0">
      <tr>
        <td>
          <table width="100%" cellpadding="0" cellspacing="1" bgcolor="#D0D0D0">
                
            <xsl:call-template name="ItemRow">
              <xsl:with-param name="CAPTION" select="'Timestamp'" />
              <xsl:with-param name="VALUE"   select="Timestamp" />
            </xsl:call-template>
                  
            <xsl:call-template name="ItemRow">
              <xsl:with-param name="CAPTION" select="'Machine Name'" />
              <xsl:with-param name="VALUE"   select="MachineName" />
            </xsl:call-template>
                  
            <xsl:call-template name="ItemRow">
              <xsl:with-param name="CAPTION" select="'OS'" />
              <xsl:with-param name="VALUE"   select="OS" />
            </xsl:call-template>
                  
            <xsl:call-template name="ItemRow">
              <xsl:with-param name="CAPTION" select="'Thread Identity'" />
              <xsl:with-param name="VALUE"   select="ThreadIdentity" />
            </xsl:call-template>
                
          </table>
        </td>
      </tr>
    </table>
    
  </xsl:template>

  
  <!-- ******************************************************************** -->
  <!-- *** ItemRow Template                                             *** -->
  <!-- ******************************************************************** -->
  <xsl:template name="ItemRow">
    <xsl:param name="CAPTION" />
    <xsl:param name="VALUE" />
    <xsl:param name="HILITE" select="0" />
            
    <tr>
      <td style="font-family: {$FONT}; font-size: 8pt; font-weight: bold; padding: 2px 5px 2px 2px" nowrap="nowrap" width="120" bgcolor="#EEEEEE" align="right" valign="top">
        <xsl:value-of select="$CAPTION" /> 
      </td>
      <td bgcolor="white" valign="top">
        <xsl:attribute name="style">
          <xsl:choose>
            <xsl:when test="$HILITE=1">
              font-family: <xsl:value-of select="$FONT" />; font-size: 8pt; padding: 2px 2px 2px 5px; font-weight: bold; color: red;
            </xsl:when>
            <xsl:otherwise>
              font-family: <xsl:value-of select="$FONT" />; font-size: 8pt; padding: 2px 2px 2px 5px;
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:value-of select="$VALUE" />
      </td>
    </tr>

  </xsl:template>

</xsl:stylesheet>
