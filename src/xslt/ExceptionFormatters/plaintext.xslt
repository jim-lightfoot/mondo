<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="text" />

  <!-- ******************************************************************** -->
  <!-- *** Main Template                                                *** -->
  <!-- ******************************************************************** -->
  <xsl:template match="/">

    <xsl:for-each select="//Exception">
--------------------------------------------------------------------------------------------
Message:          <xsl:value-of select="Message" />
Type:             <xsl:value-of select="Type" />
Source:           <xsl:value-of select="Source" />
Stack Trace:      <xsl:value-of select="StackTrace" />
<xsl:value-of select="concat('    ', '&#13;&#10;')" />
    </xsl:for-each>
    
    <xsl:for-each select="//ServerInfo">
--------------------------------------------------------------------------------------------
Timestamp:        <xsl:value-of select="Timestamp" />
Machine Name:     <xsl:value-of select="MachineName" />
OS:               <xsl:value-of select="OS" />
Thread Identity:  <xsl:value-of select="ThreadIdentity" />
<xsl:value-of select="concat('    ', '&#13;&#10;')" />
    </xsl:for-each>
    
  </xsl:template>
  
</xsl:stylesheet>
